﻿using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyRefOs : MetadataMember<MetadataRow<uint, uint,uint,uint>>
    {
        private readonly LazyValue<AssemblyReference> _reference;
        
        public AssemblyRefOs(AssemblyReference reference, uint platformId, uint majorVersion, uint minorVersion)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyRefOs))
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            _reference = new LazyValue<AssemblyReference>(reference);
        }
        
        internal AssemblyRefOs(MetadataImage image, MetadataRow<uint, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            PlatformId = row.Column1;
            MajorVersion = row.Column2;
            MinorVersion = row.Column3;
            _reference = new LazyValue<AssemblyReference>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.AssemblyRef);
                MetadataRow referenceRow;
                return table.TryGetRow((int) (row.Column4 - 1), out referenceRow)
                    ? (AssemblyReference) table.GetMemberFromRow(image, referenceRow)
                    : null;
            });
        }

        public uint PlatformId
        {
            get;
            set;
        }

        public uint MajorVersion
        {
            get;
            set;
        }

        public uint MinorVersion
        {
            get;
            set;
        }

        public AssemblyReference Reference
        {
            get { return _reference.Value;}
            set { _reference.Value = value; }
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTable<AssemblyRefOsTable>().Add(new MetadataRow<uint, uint, uint, uint>
            {
                Column1 = PlatformId,
                Column2 = MajorVersion,
                Column3 = MinorVersion,
                Column4 = Reference.MetadataToken.Rid
            });
        }
    }
}
