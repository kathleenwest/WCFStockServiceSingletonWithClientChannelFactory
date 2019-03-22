using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
	public static class ConsoleHelpers
	{
		#region Properties
		private static ConsoleColor PromptColor { get; set; }
		private static ConsoleColor InputColor { get; set; }
		private static ConsoleColor ErrorColor { get; set; }
		#endregion Properties

		#region Constructors
		/// <summary>
		/// Initialize the static fields
		/// </summary>
		static ConsoleHelpers()
		{
			// These could also have been set in the initializers for the fields...
			PromptColor = ConsoleColor.White;
			InputColor = ConsoleColor.Yellow;
			ErrorColor = ConsoleColor.Red;
		}
		#endregion Constructors

		#region Public Methods
		/// <summary>
		/// Prints a border using the given character and length
		/// </summary>
		/// <param name="ch">Character to print</param>
		/// <param name="length">Number of characters to print.  If 0 then the length is the window's width.</param>
		public static void WriteBorder(char ch = '=', int length = 0)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				Console.ForegroundColor = PromptColor;
				int row = Console.CursorTop;
				Console.SetCursorPosition(0, row);
				if (length == 0)
				{
					length = Console.WindowWidth;
				}
				Console.WriteLine(new string(ch, length));
				Console.SetCursorPosition(0, row + 1);
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a bool from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <returns>Bool value entered by user</returns>
		/// <remarks>Accepts 'y', 'yes', 'true', '1' as true, the rest false</remarks>
		public static bool ReadBool(string prompt)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				// Prompt and get user input
				Console.ForegroundColor = PromptColor;
				Console.Write(prompt);

				Console.ForegroundColor = InputColor;
				string input = Console.ReadLine().ToLower().Trim();
				if ((input == "y") || (input == "yes") || (input == "true") || (input == "1"))
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a string from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="minLength">Minimum length of the string</param>
		/// <param name="maxLength">Maximum length of string to get (0 = unlimited)</param>
		/// <returns>String value entered by user</returns>
		public static string ReadString(string prompt, int minLength = 0, int maxLength = 0)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				string input = string.Empty;
				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					done = true;
					input = Console.ReadLine();
					if ((maxLength > 0) && (input.Length > maxLength))
					{
						input = input.SafeTrim(maxLength);
					}
					if (input.Length < minLength)
					{
						// Out of range
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The text entered must be at least {0} characters in length.  Please try again.", minLength);
						done = false;
					}
				}
				return input;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a char from the user and does not require the enter key to be pressed.
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="validChars">List of chars to be accepted.  If null or empty, any character.</param>
		/// <returns>char value entered by user</returns>
		public static char ReadChar(string prompt, char[] validChars = null)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				// Prompt and get user input
				Console.ForegroundColor = PromptColor;
				Console.Write(prompt);

				bool done = false;
				char input = char.MinValue;
				while (!done)
				{
					var info = Console.ReadKey(true);
					input = info.KeyChar;

					if (validChars == null || validChars.Length == 0 || validChars.Contains(input))
					{
						
						Console.ForegroundColor = InputColor;
						Console.WriteLine(input);
						done = true;
					}
				}
				return input;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}
		
		/// <summary>
		/// Gets an byte from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Byte value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static int ReadByte(string prompt, byte lowValue = byte.MinValue, byte highValue = byte.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				byte value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (byte.TryParse(input, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not an byte.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets an int from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Integer value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static int ReadInt(string prompt, int lowValue = int.MinValue, int highValue = int.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				int value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (int.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not an integer.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a long from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Long value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static long ReadLong(string prompt, long lowValue = long.MinValue, long highValue = long.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				long value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (long.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not a long.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a decimal from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Decimal value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static decimal ReadDecimal(string prompt, decimal lowValue = decimal.MinValue, decimal highValue = decimal.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				decimal value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (decimal.TryParse(input, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not a decimal.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a float from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Float value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static double ReadFloat(string prompt, float lowValue = float.MinValue, float highValue = float.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				float value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (float.TryParse(input, NumberStyles.Number | NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not a float.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a double from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>Double value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static double ReadDouble(string prompt, double lowValue = double.MinValue, double highValue = double.MaxValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				double value = 0;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (double.TryParse(input, NumberStyles.Number | NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0} and {1}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not a double.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a date from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="lowValue">Input value must be greater than or equal to this</param>
		/// <param name="highValue">Input value must be less than or equal to this</param>
		/// <returns>DateTime value entered by the user within the expected range</returns>
		/// <remarks>This function will not exit until the user gives a valid input</remarks>
		public static DateTime ReadDate(string prompt, DateTime lowValue, DateTime highValue)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				DateTime value = DateTime.MinValue;

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.Write(prompt);

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					// Check for valid type
					if (DateTime.TryParse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AssumeLocal, out value))
					{
						// Good type, now check range
						if ((value >= lowValue) && (value <= highValue))
						{
							// This will allow the loop to exit.
							done = true;
						}
						else
						{
							// Out of range
							Console.ForegroundColor = ErrorColor;
							Console.WriteLine("The value entered was not between {0:d} and {1:d}.  Please try again.", lowValue, highValue);
						}
					}
					else
					{
						// Not the correct type
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered was not a date.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets an enum value from the user
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="enumType">Enumeration type value must belong to</param>
		/// <returns>Enum value entered by the user within the enum's range</returns>
		/// <remarks>This function will not exit until the user gives a valid input.  Also, the input can be either the number or text of the enum.</remarks>
		public static T ReadEnum<T>(string prompt) where T : struct
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				bool done = false;
				Type type = typeof(T);
				T value = default(T);

				//FlagsAttribute flags = type.GetCustomAttribute<FlagsAttribute>();
				bool flags = false;
				object[] attrs = type.GetCustomAttributes(typeof(FlagsAttribute), false);
				if (attrs != null && attrs.Length > 0)
				{
					flags = true;
				}

				while (!done)
				{
					// Prompt and get user input
					Console.ForegroundColor = PromptColor;
					Console.WriteLine(prompt);
					if (flags)
					{
						Console.WriteLine("(Separate multiple names with commas)");
					}

					// Enumerate the values for the user
					Array enumValues = Enum.GetValues(type);
					foreach (object enumValue in enumValues)
					{
						if (flags)
						{
							// Flags enum - only show names
							Console.ForegroundColor = InputColor;
							Console.WriteLine("  {0:G}", enumValue);
						}
						else
						{
							// Standard enum - show names and numbers
							Console.ForegroundColor = PromptColor;
							Console.Write("  [");
							Console.ForegroundColor = InputColor;
							Console.Write("{0:D}", enumValue);
							Console.ForegroundColor = PromptColor;
							Console.Write("] - ");
							Console.ForegroundColor = InputColor;
							Console.WriteLine("{0:G}", enumValue);
						}
					}
					Console.ForegroundColor = PromptColor;
					Console.Write("Value: ");

					Console.ForegroundColor = InputColor;
					string input = Console.ReadLine();

					if (Enum.TryParse(input, true, out value))
					{
						if (flags)
						{
							// Since this is a flag enum, there is the possibility the value is not defined, but just a valid number
							if (EnumValueIsDefined(type, value))
							{
								done = true;
							}
						}
						else
						{
							// For non-flags enum, just check the value is defined by the enum
							if (Enum.IsDefined(type, value))
							{
								done = true;
							}
						}
					}
					if (!done)
					{
						// Out of range
						Console.ForegroundColor = ErrorColor;
						Console.WriteLine("The value entered does not exist in the value list.  Please try again.");
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		/// <summary>
		/// Gets a char from the user and does not require the enter key to be pressed.
		/// </summary>
		/// <param name="prompt">Prompt to display to the user</param>
		/// <param name="items">List of items to choose from</param>
		/// <returns>Item selected</returns>
		public static object PickList(string prompt, IList items)
		{
			// Remember original console color so we can set it back when exiting
			ConsoleColor originalColor = Console.ForegroundColor;

			try
			{
				object value = null;
				
				// Prompt and get user input
				Console.ForegroundColor = PromptColor;
				Console.WriteLine(prompt);
					
				// Enumerate the values for the user
				int index = 0;
				string fmt = "  {0,1}. ";
				if (items.Count >= 10 && items.Count < 100)
				{
					fmt = "  {0,2}. ";
				}
				else if (items.Count >= 100 && items.Count < 1000)
				{
					fmt = "  {0,3}. ";
				}

				for (int i = 0; i < items.Count; i++)
				{
					Console.ForegroundColor = PromptColor;
					Console.Write(fmt, i + 1);
					Console.ForegroundColor = InputColor;
					Console.WriteLine(items[i]);
				}
				index = ReadInt("Value: ", 1, items.Count);
				value = items[index - 1];
				return value;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.ForegroundColor = ErrorColor;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}
		#endregion Public Methods

		#region Private Methods
		/// <summary>
		/// Determines if the given flags enum value is defined in the given enum
		/// </summary>
		/// <param name="enumType">Enum type</param>
		/// <param name="value">Value to check for</param>
		/// <returns>true if the enum value is defined, false otherwise</returns>
		private static bool EnumValueIsDefined(Type enumType, object value)
		{
			try
			{
				ulong bits = Convert.ToUInt64(value);
				List<ulong> definedFlags = new List<ulong>();
				Array values = Enum.GetValues(enumType);

				foreach (object o in values)
				{
					definedFlags.Add(Convert.ToUInt64(o));
				}

				if (bits == 0 && definedFlags.Contains(0))
				{
					return true;
				}

				// Use a ulong to test for bits set - negative flags do not make logical sense and ulong is the largest enum size
				for (int i = 0; i < 64; i++)
				//for (ulong bit = 0x1; bit <= 0x8000000000000000; bit <<= 1)
				{
					// Check to see if the current flag exists in the bit mask
					ulong bit = 0x1UL << i;
					if ((bits & bit) != 0)
					{
						// Check that the enum defined this bit
						if (!definedFlags.Contains(bit))
						{
							return false;
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
				System.Diagnostics.Debug.WriteLine(ex.Message, string.Format("{0}.{1}.{2}", mb.DeclaringType.Namespace, mb.DeclaringType.Name, mb.Name));
				return false;
			}
		}
		#endregion Private Methods

	}
}
