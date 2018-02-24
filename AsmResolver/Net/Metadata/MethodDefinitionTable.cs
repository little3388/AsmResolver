﻿using System;
using System.Data;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodDefinitionTable : MetadataTable<MetadataRow<RvaDataSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Method; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) + // Rva
                       sizeof(ushort) + // ImplAttrbibutes
                       sizeof(ushort) + // Attributes
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.BlobIndexSize + // Signature
                       (uint) TableStream.GetTable(MetadataTokenType.Param).IndexSize; // ParamList
            }
        }

        protected override MetadataRow<RvaDataSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            
            uint rva = reader.ReadUInt32();
            var implAttributes = (MethodImplAttributes) reader.ReadUInt16();
            
            RvaDataSegment body = null;
            if (rva != 0)
            {
                long fileOffset = context.Assembly.RvaToFileOffset(rva);
                if (implAttributes.HasFlag(MethodImplAttributes.IL))
                {
                    int size = CilMethodBody.GetMethodBodySize(context.CreateSubContext(fileOffset));
                    body = new RvaDataSegment(rva, context.Reader.CreateSubReader(fileOffset, size));
                }
                else
                {
                    body = new RvaDataSegment(rva, context.Reader.CreateSubReader(fileOffset, 0));
                }
            }

            return new MetadataRow<RvaDataSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint>(token)
            {
                Column1 = body,                                                          
                Column2 = implAttributes,                                                           // ImplAttrbibutes
                Column3 = (MethodAttributes) reader.ReadUInt16(),                                   // Attributes
                Column4 = reader.ReadIndex(TableStream.StringIndexSize),                            // Name
                Column5 = reader.ReadIndex(TableStream.BlobIndexSize),                              // Signature
                Column6 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Param).IndexSize) // ParamList
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<RvaDataSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1 != null ? row.Column1.Rva : 0);
            writer.WriteUInt16((ushort) row.Column2);
            writer.WriteUInt16((ushort) row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column5);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Param).IndexSize, row.Column6);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<RvaDataSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
        {
            return new MethodDefinition(image, row);
        }
    }
}