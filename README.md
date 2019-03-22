# WCFStockServiceSingletonWithClientChannelFactory

A WCF Stock Service (Singleton) Library, Service Host Console Application, and a “Tester” Client (ChannelFactory) Console Application

Project Blog Article here: https://portfolio.katiegirl.net/2019/03/22/a-wcf-stock-service-singleton-library-service-host-console-application-and-a-tester-client-channelfactory-console-application/

About

This project presents a WCF Stock Service Library (StockServiceLib) that mimics a stock exchange. The service is implemented as a “singleton” and maintains persistent data between client calls and can handle multiple client sessions. The service is hosted via a console application (StockServiceHost). The client and service participate in a bi-directional/callback relationship. The client (StockClient) uses the ChannelFactory pattern as opposed to “Add Service Reference” with SVCUTIL. The client and service share a common assembly (SharedLib) that contains the key contract and data model information. Furthermore, a Utilities project is used by the client console application to facilitate user data entry and the complicated details of building and managing the WCF ChannelFactory connection implementation. The ProxyGen class inside the Utilities project abstracts the details of implementing and managing a generic ChannelFactory connection to a generic service for a client. Note: The Utilities project library was included as base code for my lab project to facilitate speedy completion; we were not expected to code this Utilities project ourselves due to complexity and time constraints. The remaining projects in the solution (SharedLib, StockClient, StockServiceHost, and StockServiceLib), I completed individually per requirements for the lab project.

Architecture Overview
The StockServiceDemo Visual Studio solution application consists of five project assemblies:
•	SharedLib (Class Library) “Service and Data Contracts”
•	StockClient (Console Application) “Client Tester to Service”
•	StockServiceHost (Console Application) “Hosts the Service”
•	StockServiceLib (WCF Service Library) “Implements the Service”
•	Utilities (Class Library) “Helper classes”

