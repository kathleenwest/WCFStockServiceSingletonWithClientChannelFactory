using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.MsmqIntegration;

namespace Utilities
{
	public static class ProxyGen
	{
		#region Fields
		private static Dictionary<string, object> m_Channels = new Dictionary<string, object>();
		private static string m_Host = string.Empty;
		private static TimeSpan m_Expiration = TimeSpan.FromMinutes(10);
		#endregion Fields

		#region Constructors
		static ProxyGen()
		{
			try
			{
				Host = GetHostAddress();
			}
			catch (Exception ex)
			{
				System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
			}
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// Cached channel proxies
		/// </summary>
		private static Dictionary<string, object> Channels
		{
			get
			{
				return ProxyGen.m_Channels;
			}
			set
			{
				ProxyGen.m_Channels = value;
			}
		}

		/// <summary>
		/// Host
		/// </summary>
		public static string Host
		{
			get
			{
				return m_Host;
			}
			set
			{
				ProxyGen.m_Host = value;
			}
		}
		#endregion Properties

		#region Public Methods
		/// <summary>
		/// Gets a reference to the requested service object
		/// </summary>
		/// <typeparam name="T">Contract to create proxy for</typeparam>
		/// <returns>New service object reference</returns>
		public static T GetChannel<T>()
		{
			return GetChannel<T>(null);
		}

		/// <summary>
		/// Gets a reference to the requested duplex service object
		/// </summary>
		/// <typeparam name="T">Contract to create proxy for</typeparam>
		/// <returns>New service object reference</returns>
		public static T GetChannel<T>(object callback)
		{
			try
			{
				Type type = typeof(T);
				string typeName = type.FullName;
				IContextChannel channel = default(T) as IContextChannel;

				// Check if we have already created this channel in the past
				if (Channels.ContainsKey(typeName))
				{
					//dynamic container = Channels[typeName];
					//// Calling the Container's Channel property check for valid channel
					//channel = container.Channel;
					object container = Channels[typeName];
					Type t = container.GetType();
					PropertyInfo pi = t.GetProperty("Channel", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
					channel = (IContextChannel)pi.GetValue(container, null);
				}
				else
				{
					ChannelEndpointElement endPoint = GetObjectAddress(typeName);
					if (endPoint != null)
					{
						// Create the endpoint address and binding for the contract
						EndpointAddress address = new EndpointAddress(endPoint.Address.AbsoluteUri);
						Binding binding = GetBinding(endPoint);
						if (binding == null)
						{
							throw new Exception("Unknown Wcf Binding type: " + endPoint.Binding);
						}

						// Create the channel factory and create the channel from that factory
						ChannelFactory<T> factory = null;
						if (callback != null)
						{
							factory = new DuplexChannelFactory<T>(callback, binding, address);
						}
						else
						{
							factory = new ChannelFactory<T>(binding, address);
						}

						//
						// TODO: Perform any customization to the factory here
						//
						FixupFactory(factory);

						//
						// TODO: Perform any customization to the channel here
						//       such as OperationTimeout
						//

						// Store channel for later use
						ChannelContainer<T> ct = new ChannelContainer<T>(factory);
						channel = ct.Channel;
						Channels[typeName] = ct;
					}
					else
					{
						throw new Exception(string.Format("Unable to determine the Wcf endpoint for: {0}", typeName));
					}

				}
				return (T)channel;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
				throw;
			}
		}

		/// <summary>
		/// Abort the specified channel.
		/// </summary>
		/// <param name="channel">The channel to close.</param>
		public static void Abort<T>(T channel)
		{
			Type type = typeof(T);
			string typeName = type.FullName;
			if (Channels.ContainsKey(typeName))
			{
				Channels.Remove(typeName);
			}
			CloseChannel(channel as IContextChannel, true);
		}


		/// <summary>
		/// Close the specified channel.
		/// </summary>
		/// <param name="channel">The channel to close.</param>
		public static void Close<T>(T channel)
		{
			Type type = typeof(T);
			string typeName = type.FullName;
			if (Channels.ContainsKey(typeName))
			{
				Channels.Remove(typeName);
			}
			CloseChannel(channel as IContextChannel, false);
		}
		#endregion Public Methods

		#region Private Methods
		/// <summary>
		/// Applies customization to the given factory object
		/// </summary>
		/// <param name="factory">Factory object to fixup</param>
		private static void FixupFactory(ChannelFactory factory)
		{
			try
			{
				// *********************
				// WORKAROUND:
				// *********************
				// The endpoint behavior in the config file is not being applied to the operation contratcs correctly.
				// Manually apply the settings from the first endpoint behavior in the config file.
				// *********************
				EndpointBehaviorElement behavior = GetEndpointBehavior(string.Empty);
				DataContractSerializerElement dcs = null;
				if (behavior != null)
				{
					foreach (BehaviorExtensionElement obj in behavior)
					{
						if (obj is DataContractSerializerElement)
						{
							dcs = obj as DataContractSerializerElement;
							break;
						}
					}
				}
				if (dcs != null)
				{
					foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
					{
						DataContractSerializerOperationBehavior dcb = op.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
						if (dcb != null)
						{
							dcb.MaxItemsInObjectGraph = dcs.MaxItemsInObjectGraph;
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
				throw;
			}
		}

		/// <summary>
		/// Retrieves the Uri for a given WCF Service
		/// </summary>
		/// <param name="name">Name of the endpoint behavior to serach for (string.Empty for first entry)</param>
		/// <returns>Endpoint behavior element found, or null</returns>
		private static EndpointBehaviorElement GetEndpointBehavior(string name)
		{
			try
			{
				Configuration conf =
				  ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);

				ServiceModelSectionGroup svcmod =
				  (ServiceModelSectionGroup)conf.GetSectionGroup("system.serviceModel");

				foreach (EndpointBehaviorElement behavior in svcmod.Behaviors.EndpointBehaviors)
				{
					if ((behavior.Name == name) || (name == string.Empty))
					{
						return behavior;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
			}
			return null;
		}

		/// <summary>
		/// Determines if the context channel object is in an alive state
		/// </summary>
		/// <param name="channel">Channel to test</param>
		/// <returns>true if alive, false otherwise</returns>
		private static bool IsAlive(IContextChannel channel)
		{
			try
			{
				if (channel == null)
				{
					return false;
				}
				CommunicationState state = channel.State;
				switch (state)
				{
					case CommunicationState.Closed:
					case CommunicationState.Closing:
					case CommunicationState.Faulted:
						return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
				return false;
			}
		}

		/// <summary>
		/// Creates the appropriate binding type for the given element
		/// </summary>
		/// <param name="endPoint">Enpoint to create binding object for</param>
		/// <returns>Binding represented in the config file</returns>
		private static Binding GetBinding(ChannelEndpointElement endPoint)
		{
			Binding binding = null;

			try
			{
				switch (endPoint.Binding)
				{
					case "basicHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new BasicHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new BasicHttpBinding();
						}
						break;
					case "basicHttpContextBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new BasicHttpContextBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new BasicHttpContextBinding();
						}
						break;
					case "customBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new CustomBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new CustomBinding();
						}
						break;
					case "netNamedPipeBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetNamedPipeBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetNamedPipeBinding();
						}
						break;
					case "netPeerTcpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetPeerTcpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetPeerTcpBinding();
						}
						break;
					case "netTcpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetTcpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetTcpBinding();
						}
						break;
					case "netTcpContextBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetTcpContextBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetTcpContextBinding();
						}
						break;
					case "webHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WebHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WebHttpBinding();
						}
						break;
					case "ws2007HttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WS2007HttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WS2007HttpBinding();
						}
						break;
					case "wsHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WSHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WSHttpBinding();
						}
						break;
					case "wsHttpContextBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WSHttpContextBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WSHttpContextBinding();
						}
						break;
					case "wsDualHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WSDualHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WSDualHttpBinding();
						}
						break;
					case "wsFederationHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WSFederationHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WSFederationHttpBinding();
						}
						break;
					case "ws2007FederationHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new WS2007FederationHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new WS2007FederationHttpBinding();
						}
						break;
					case "msmqIntegrationBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new MsmqIntegrationBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new MsmqIntegrationBinding();
						}
						break;
					case "netMsmqBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetMsmqBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetMsmqBinding();
						}
						break;
					case "netHttpBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetHttpBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetHttpBinding();
						}
						break;
					case "netHttpsBinding":
						if (!string.IsNullOrEmpty(endPoint.BindingConfiguration))
						{
							binding = new NetHttpsBinding(endPoint.BindingConfiguration);
						}
						else
						{
							binding = new NetHttpBinding();
						}
						break;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
				throw ex;
			}

			return binding;
		}

		/// <summary>
		/// Gets the system.serviceModel group from the config file
		/// </summary>
		/// <returns></returns>
		private static ServiceModelSectionGroup GetServiceModelSectionGroup()
		{
			ServiceModelSectionGroup svcmod = null;
			try
			{
				AppDomain appDomain = AppDomain.CurrentDomain;
				if (appDomain != null)
				{
					if (appDomain.SetupInformation.ConfigurationFile.Contains("web.config"))
					{
						// Web application
						var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
						svcmod = (ServiceModelSectionGroup)webConfig.GetSectionGroup("system.serviceModel");
					}
					else
					{
						// Stand-alone application
						Configuration conf = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
						svcmod = (ServiceModelSectionGroup)conf.GetSectionGroup("system.serviceModel");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
				throw ex;
			}
			return svcmod;
		}

		/// <summary>
		/// Retrieves the ChannelEndpointElement for a given WCF Service
		/// </summary>
		/// <param name="name">Service contract to search for</param>
		/// <returns>ChannelEndpointElement</returns>
		private static ChannelEndpointElement GetObjectAddress(string name)
		{
			try
			{
				ServiceModelSectionGroup svcmod = GetServiceModelSectionGroup();

				foreach (ChannelEndpointElement endpoint in svcmod.Client.Endpoints)
				{
					if (name == endpoint.Contract)
					{
						Uri uri = new Uri(string.Format("{0}://{1}:{2}{3}", endpoint.Address.Scheme, Host, endpoint.Address.Port, endpoint.Address.LocalPath));
						endpoint.Address = uri;
						return endpoint;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
				throw ex;
			}
			return null;
		}

		/// <summary>
		/// Retrieves the host address for the first endpoint
		/// </summary>
		/// <returns>Host name or address</returns>
		private static string GetHostAddress()
		{
			try
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				asm = Assembly.GetEntryAssembly();
				asm = Assembly.GetCallingAssembly();
				AssemblyName[] names = asm.GetReferencedAssemblies();

				ServiceModelSectionGroup svcmod = GetServiceModelSectionGroup();

				foreach (ChannelEndpointElement endpoint in svcmod.Client.Endpoints)
				{
					return endpoint.Address.Host;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
				throw ex;
			}
			return string.Empty;
		}

		/// <summary>
		/// Close the specified service channel.
		/// </summary>
		/// <param name="channel">The channel to close.</param>
		/// <param name="abort">Whether the close should be an abort.</param>
		private static void CloseChannel(IContextChannel channel, bool abort)
		{
			if (channel != null)
			{
				if (abort)
				{
					channel.Abort();
				}
				else
				{
					channel.Close();
				}
			}
		}
		#endregion Private Methods

		#region Event Handlers
		/// <summary>
		/// A channel has transitioned to the faulted state
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void channel_Faulted(object sender, EventArgs e)
		{
			try
			{
				IContextChannel channel = sender as IContextChannel;
				if (channel != null)
				{
					string typeName = channel.GetType().FullName;
					if (Channels.ContainsKey(typeName))
					{
						Channels.Remove(typeName);
					}

					System.Diagnostics.Debug.WriteLine(string.Format("{0} is in the faulted state.", typeName), string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
					try
					{
						channel.Abort();
					}
					catch
					{
						// No op...
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", MethodBase.GetCurrentMethod().DeclaringType.Namespace, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name));
			}
		}
		#endregion Event Handlers



		private class ChannelContainer<T>
		{
			#region Fields
			private static TimeSpan m_Expiration = TimeSpan.FromMinutes(10);
			private ChannelFactory<T> m_Factory = null;
			private IContextChannel m_Channel = null;
			private DateTime m_LastAccess = DateTime.MinValue;
			#endregion Fields

			#region Properties
			/// <summary>
			/// Factory
			/// </summary>
			public ChannelFactory<T> Factory
			{
				get
				{
					return m_Factory;
				}
				private set
				{
					m_Factory = value;
				}
			}

			/// <summary>
			/// LastAccess
			/// </summary>
			public DateTime LastAccess
			{
				get
				{
					return m_LastAccess;
				}
				private set
				{
					m_LastAccess = value;
				}
			}

			/// <summary>
			/// Channel
			/// </summary>
			public IContextChannel Channel
			{
				get
				{
					try
					{
						if (m_Channel == null)
						{
							Open();
						}
						else if (LastAccess.Add(m_Expiration) < DateTime.Now)
						{
							// Refresh the channel
							System.Diagnostics.Trace.WriteLine("Channel expired '" + typeof(T).Name + "'");
							Refresh();
						}
						LastAccess = DateTime.Now;
						return m_Channel;
					}
					catch (Exception ex)
					{
						System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
						System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
						throw;
					}
				}
				private set
				{
					m_Channel = value;
				}
			}
			#endregion Properties

			#region Constructors
			/// <summary>
			/// Construct the channel container using the given factory
			/// </summary>
			/// <param name="factory">Factory used for constructing the container</param>
			public ChannelContainer(ChannelFactory<T> factory)
			{
				Factory = factory;
			}
			#endregion Constructors

			#region Methods
			/// <summary>
			/// Refreshes the channel
			/// </summary>
			private void Refresh()
			{
				try
				{
					Close();
					Open();
				}
				catch (Exception ex)
				{
					System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
					System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
					throw;
				}
			}

			/// <summary>
			/// Opens the channel
			/// </summary>
			private void Open()
			{
				try
				{
					System.Diagnostics.Trace.WriteLine("Opening channel '" + typeof(T).Name + "'");
					IContextChannel ch = (IContextChannel)Factory.CreateChannel();
					ch.Open();
					ch.Faulted += new EventHandler(Channel_Faulted);
					Channel = ch;
				}
				catch (Exception ex)
				{
					System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
					System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
					throw;
				}
			}

			/// <summary>
			/// Closes the channel
			/// </summary>
			private void Close()
			{
				try
				{
					System.Diagnostics.Trace.WriteLine("Closing channel '" + typeof(T).Name + "'");
					// Access m_Channel directly because accessing Channel can cause stack overflow
					if (m_Channel != null)
					{
						// Kill existing channel
						m_Channel.Faulted -= new EventHandler(Channel_Faulted);
						try
						{
							m_Channel.Close(TimeSpan.FromMilliseconds(100));
						}
						catch (Exception ex)
						{
							System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
							System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
							try
							{
								m_Channel.Abort();
							}
							catch
							{
								// No-op
							}
						}
					}
					Channel = null;
				}
				catch (Exception ex)
				{
					System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
					System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
				}
			}
			#endregion Methods

			#region Event Handlers
			/// <summary>
			/// Fired when the channel is faulted
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			void Channel_Faulted(object sender, EventArgs e)
			{
				try
				{
					System.Diagnostics.Trace.WriteLine("Channel " + typeof(T).Name + " faulted.");
					// Close and re-open the channel
					Refresh();
				}
				catch (Exception ex)
				{
					System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
					System.Diagnostics.Trace.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
				}
			}
			#endregion Event Handlers
		}
	}
}
