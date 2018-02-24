﻿using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class MethodImplementation : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<IMethodDefOrRef> _methodBody;
        private readonly LazyValue<IMethodDefOrRef> _methodDeclaration;

        public MethodImplementation(TypeDefinition @class, IMethodDefOrRef methodBody,
            IMethodDefOrRef methodDeclaration)
            : base(null, new MetadataToken(MetadataTokenType.MethodImpl))
        {
            _class = new LazyValue<TypeDefinition>(@class);
            _methodBody = new LazyValue<IMethodDefOrRef>(methodBody);
            _methodDeclaration = new LazyValue<IMethodDefOrRef>(methodDeclaration);
        }

        internal MethodImplementation(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();
            var encoder = tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);

            _class = new LazyValue<TypeDefinition>(() =>
            {
                var table = tableStream.GetTable(MetadataTokenType.TypeDef);
                MetadataRow typeRow;
                return table.TryGetRow((int) (row.Column1 - 1), out typeRow) 
                    ? (TypeDefinition) table.GetMemberFromRow(image, typeRow) 
                    : null;
            });

            _methodBody = new LazyValue<IMethodDefOrRef>(() =>
            {
                var methodBodyToken = encoder.DecodeIndex(row.Column2);
                IMetadataMember member;
                return image.TryResolveMember(methodBodyToken, out member) ? (IMethodDefOrRef) member : null;
            });

            _methodDeclaration = new LazyValue<IMethodDefOrRef>(() =>
            {
                var declarationToken = encoder.DecodeIndex(row.Column3);
                IMetadataMember member;
                return image.TryResolveMember(declarationToken, out member) ? (IMethodDefOrRef) member : null;
            });
        }

        public TypeDefinition Class
        {
            get { return _class.Value; }
            set { _class.Value = value; }
        }

        public IMethodDefOrRef MethodBody
        {
            get { return _methodBody.Value; }
            set { _methodBody.Value = value; }
        }

        public IMethodDefOrRef MethodDeclaration
        {
            get { return _methodDeclaration.Value; }
            set { _methodDeclaration.Value = value; }
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            var encoder = tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            tableStream.GetTable<MethodImplementationTable>().Add(new MetadataRow<uint, uint, uint>
            {
                Column1 = Class.MetadataToken.Rid,
                Column2 = encoder.EncodeToken(MethodBody.MetadataToken),
                Column3 = encoder.EncodeToken(MethodDeclaration.MetadataToken)
            });
        }
    }
}