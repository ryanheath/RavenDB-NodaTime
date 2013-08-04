Noda Time support for RavenDB
=============================

This is a custom extension for RavenDB.  

It allows you to use data types from [Noda Time](http://www.nodatime.org), such as `Instant`, `LocalDateTime`, and others.
Specifically, it enables you to use these types directly in your domain entities, which get serialized into RavenDB documents.

The current release requires RavenDB version 2.5.2666 or higher.  However, if you are sorting on `LocalTime`, `Offset`, or `Duration` types, then you need to use
RavenDB 2.5.2670 or higher, due to [an issue](https://github.com/mj1856/RavenDB-NodaTime/issues/1) that
was resolved in that build.

Full documentation is pending.  Please review the unit tests for example usage.


### Nuget Installation

    PM> Install-Package RavenDB.Client.NodaTime

### Manual Installation

- Download the `Raven.Client.NodaTime.dll` file from [here](https://github.com/mj1856/RavenDB-NodaTime/releases).
- Add a reference to it in your application.

### Configuration

You need to add a single line to your code, right after the initialization call.

    documentStore.Initialize();            // you should already have this
    documentStore.ConfigureForNodaTime();  // add this single line

You can now use Noda Time types and they should be understood by the RavenDB client.

### Current Limitations

This is only a client-side extension.  It does not operate within the RavenDB server context.
You can still use these types in index definitions, but they will be treated as their non-Noda equivalents when RavenDB executes the index.
This means that you cannot execute any methods or traverse any properties from within the index.

Work is in-progress to add a server-side bundle as a companion to this library, which would alleviate these limitations.
