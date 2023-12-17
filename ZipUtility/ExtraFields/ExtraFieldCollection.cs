using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace ZipUtility.ExtraFields
{
    internal class ExtraFieldCollection
        : IExtraFieldCollection
    {
        private class InternalExtraFieldItem
        {
            public InternalExtraFieldItem(UInt16 extraFieldId, ZipEntryHeaderType appliedHeaderType, ReadOnlyMemory<Byte> extraFieldBody)
            {
                ExtraFieldId = extraFieldId;
                AppliedHeaderType = appliedHeaderType;
                ExtraFieldBody = extraFieldBody;
            }

            public UInt16 ExtraFieldId { get; }
            public ZipEntryHeaderType AppliedHeaderType { get; }
            public ReadOnlyMemory<Byte> ExtraFieldBody { get; }
        }

        private class DecodingParameter
            : IExtraFieldDecodingParameter
        {
            private readonly ValidationStringency _stringency;

            public DecodingParameter(ValidationStringency stringency)
            {
                _stringency = stringency;
            }

            ValidationStringency IExtraFieldDecodingParameter.Stringency => _stringency;
        }

        private class EncodingParameter
            : IExtraFieldEncodingParameter
        {
        }

        private readonly ZipEntryHeaderType _headerType;
        private readonly IDictionary<UInt16, InternalExtraFieldItem> _extraFields;

        /// <summary>
        /// 指定されたヘッダの拡張フィールドを保持し、拡張フィールドの初期値のデータソースとして空のデータが与えられたコンストラクタ
        /// </summary>
        /// <param name="headerType">
        /// 対応するヘッダのIDを表す <see cref="ZipEntryHeaderType"/> 値
        /// </param>
        public ExtraFieldCollection(ZipEntryHeaderType headerType)
            : this(headerType, new Dictionary<UInt16, InternalExtraFieldItem>())
        {
        }

        /// <summary>
        /// 指定された <see cref="ExtraFieldCollection"/> オブジェクトを複製するコンストラクタ
        /// </summary>
        /// <param name="source">
        /// 複製元の <see cref="ExtraFieldCollection"/> オブジェクト
        /// </param>
        public ExtraFieldCollection(ExtraFieldCollection source)
            : this(source._headerType, source._extraFields.ToDictionary(item => item.Key, item => item.Value))
        {
        }

        /// <summary>
        /// 指定されたヘッダの拡張フィールドを保持し、拡張フィールドの初期値のデータソースとしてバイトシーケンスが与えられたコンストラクタ
        /// </summary>
        /// <param name="headerType">
        /// 対応するヘッダのIDを表す <see cref="ZipEntryHeaderType"/> 値
        /// </param>
        /// <param name="extraFieldsSource">
        /// 拡張フィールドの初期値のデータソースとして与えられた <see cref="ReadOnlyMemory{Byte}">ReadOnlyMemory&lt;<see cref="Byte"/>&gt;</see> オブジェクト。
        /// </param>
        public ExtraFieldCollection(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> extraFieldsSource)
            : this(headerType, new Dictionary<UInt16, InternalExtraFieldItem>())
        {
            AppendExtraFields(extraFieldsSource);
        }

        private ExtraFieldCollection(ZipEntryHeaderType headerType, IDictionary<UInt16, InternalExtraFieldItem> extraFields)
        {
            _headerType = headerType;
            _extraFields = extraFields;
        }

        /// <inheritdoc/>
        public void AddExtraField<EXTRA_FIELD_T>(EXTRA_FIELD_T extraField)
            where EXTRA_FIELD_T : IExtraField
        {
            var body = extraField.GetData(_headerType, new EncodingParameter());
            if (body is null)
                return;
            if (body.Value.Length > UInt16.MaxValue)
                throw new OverflowException($"Too large extra field data in {extraField.GetType().FullName}");
            _extraFields[extraField.ExtraFieldId] = new InternalExtraFieldItem(extraField.ExtraFieldId, _headerType, body.Value);
        }

        /// <inheritdoc/>
        public void Clear() => _extraFields.Clear();

        /// <inheritdoc/>
        public void Delete(UInt16 extraFieldId) => _ = _extraFields.Remove(extraFieldId);

        /// <inheritdoc/>
        public Boolean Contains(UInt16 extraFieldId) => _extraFields.ContainsKey(extraFieldId);

        /// <inheritdoc/>
        public EXTRA_FIELD_T? GetExtraField<EXTRA_FIELD_T>(ValidationStringency stringency)
            where EXTRA_FIELD_T : class, IExtraField, new()
        {
            var extraField = new EXTRA_FIELD_T();
            if (!_extraFields.TryGetValue(extraField.ExtraFieldId, out InternalExtraFieldItem? sourceData))
                return null;
            extraField.SetData(sourceData.AppliedHeaderType, sourceData.ExtraFieldBody, new DecodingParameter(stringency));
            return extraField;
        }

        /// <inheritdoc/>
        public ReadOnlyMemory<Byte> ToByteArray()
        {
            var bufferLength =
                _extraFields.Values
                .Aggregate(
                    (UInt16)0,
                    (value, extraField) =>
                        checked((UInt16)(value + sizeof(UInt16) + sizeof(UInt16) + extraField.ExtraFieldBody.Span.Length)));
            var builder = new ByteArrayBuilder(bufferLength);
            foreach (var extraFieldItem in _extraFields.Values)
            {
                builder.AppendUInt16LE(extraFieldItem.ExtraFieldId);
                builder.AppendUInt16LE((UInt16)extraFieldItem.ExtraFieldBody.Length);
                builder.AppendBytes(extraFieldItem.ExtraFieldBody.Span);
            }

            return builder.ToByteArray();
        }

        /// <inheritdoc/>
        public IEnumerable<UInt16> EnumerateExtraFieldIds() => _extraFields.Keys.ToList();

        /// <inheritdoc/>
        public Int32 Count => _extraFields.Count;

        private void AppendExtraFields(ReadOnlyMemory<Byte> extraFieldsSource)
        {
            var reader = new ByteArrayReader(extraFieldsSource);
            while (!reader.IsEmpty)
            {
                try
                {
                    var extraFieldId = reader.ReadUInt16LE();
                    var extraFieldBodyLength = reader.ReadUInt16LE();
                    var extraFieldBody = reader.ReadBytes(extraFieldBodyLength);
                    _extraFields[extraFieldId] = new InternalExtraFieldItem(extraFieldId, _headerType, extraFieldBody);
                }
                catch (UnexpectedEndOfBufferException ex)
                {
                    throw
                        new BadZipFileFormatException(
                            $"Can not parse extra fields: header='{_headerType}', extra data='{extraFieldsSource.ToArray().ToFriendlyString()}'",
                            ex);
                }
            }
        }
    }
}
