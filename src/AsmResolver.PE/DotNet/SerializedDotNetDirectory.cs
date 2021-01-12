using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides an implementation of a .NET directory that was stored in a PE file.
    /// </summary>
    public class SerializedDotNetDirectory : DotNetDirectory
    {
        private readonly PEReadContext _context;
        private readonly DataDirectory _metadataDirectory;
        private readonly DataDirectory _resourcesDirectory;
        private readonly DataDirectory _strongNameDirectory;
        private readonly DataDirectory _codeManagerDirectory;
        private readonly DataDirectory _vtableFixupsDirectory;
        private readonly DataDirectory _exportsDirectory;
        private readonly DataDirectory _nativeHeaderDirectory;

        /// <summary>
        /// Reads a .NET directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments are <c>null</c>.</exception>
        public SerializedDotNetDirectory(PEReadContext context, IBinaryStreamReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Offset = reader.Offset;

            uint cb = reader.ReadUInt32();
            MajorRuntimeVersion = reader.ReadUInt16();
            MinorRuntimeVersion = reader.ReadUInt16();
            _metadataDirectory = DataDirectory.FromReader(reader);
            Flags = (DotNetDirectoryFlags) reader.ReadUInt32();
            Entrypoint = reader.ReadUInt32();
            _resourcesDirectory = DataDirectory.FromReader(reader);
            _strongNameDirectory = DataDirectory.FromReader(reader);
            _codeManagerDirectory = DataDirectory.FromReader(reader);
            _vtableFixupsDirectory = DataDirectory.FromReader(reader);
            _exportsDirectory = DataDirectory.FromReader(reader);
            _nativeHeaderDirectory = DataDirectory.FromReader(reader);
        }

        /// <inheritdoc />
        protected override IMetadata GetMetadata()
        {
            if (_metadataDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_metadataDirectory, out var directoryReader))
            {
                return new SerializedMetadata(_context, directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override DotNetResourcesDirectory GetResources()
        {
            if (_resourcesDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_resourcesDirectory, out var directoryReader))
            {
                return new SerializedDotNetResourcesDirectory(directoryReader);
            }

            return null;

        }

        /// <inheritdoc />
        protected override IReadableSegment GetStrongName()
        {
            if (_strongNameDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_strongNameDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetCodeManagerTable()
        {
            if (_codeManagerDirectory.IsPresentInPE
            && _context.File.TryCreateDataDirectoryReader(_codeManagerDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetVTableFixups()
        {
            if (_vtableFixupsDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_vtableFixupsDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetExportAddressTable()
        {
            if (_exportsDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_exportsDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetManagedNativeHeader()
        {
            if (_nativeHeaderDirectory.IsPresentInPE
                && _context.File.TryCreateDataDirectoryReader(_nativeHeaderDirectory, out var directoryReader))
            {
                // TODO: interpretation instead of raw contents.
                return DataSegment.FromReader(directoryReader);
            }

            return null;
        }

    }
}