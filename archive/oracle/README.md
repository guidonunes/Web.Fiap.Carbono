# Archived Oracle implementation

This directory preserves the former Oracle/Entity Framework Core context,
repositories, services, and migrations for historical reference and recovery.
They are not compiled into the API project and are not part of the runtime
persistence path. The one-off `Web.Fiap.Carbono.Migration` console links only
the archived `DatabaseContext` when an Oracle-to-MongoDB migration is required.

The active API uses the MongoDB driver exclusively.
