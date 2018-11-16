﻿/// tsc 빌드시에 node.d.ts 파일을 포함시켜주시기 바랍니다.

import fs = require("fs");

enum DataLocation {
    Both,
    Server,
    Client,
}

enum SeekOrigin {
    Begin,
    Current,
    End,
}

export function readFromFile(filename: string): IDataSet {
    return CremaReader.readFromFile(filename);
}

export class CremaReader {
    public static readFromFile(filename: string): IDataSet {
        if (fs.existsSync(filename) === false) {
            throw new Error("file not exist : " + filename);
        }

        let stats: fs.Stats = fs.statSync(filename);
        let fd: number = fs.openSync(filename, "r");
        let buffer: Buffer = new Buffer(stats.size);

        fs.readSync(fd, buffer, 0, stats.size, null);
        let reader: BufferReader = new BufferReader(buffer);
        let cremaReader: BinaryReader = new BinaryReader();
        cremaReader.read(reader);

        fs.closeSync(fd);
        return cremaReader;
    }
}

class StringResource {
    private static strings: { [s: number]: string; } = {};

    public static read(reader: BufferReader): void {
        let stringCount: number = reader.readInt32();

        for (let i: number = 0; i < stringCount; i++) {
            let id: number = reader.readInt32();
            let length: number = reader.readInt32();

            if (this.strings[id] == null) {
                let text: string = reader.readString(length);
                this.strings[id] = text;
            } else {
                reader.seek(length, SeekOrigin.Current);
            }
        }
    }

    public static getString(id: number): string {
        return this.strings[id];
    }
}

class TableHeader {
    public static defaultMagicValue: number = 0x04000000;

    public magicValue: number;
    public hashValue: number;
    public modifiedTime: number;
    public tableInfoOffset: number;
    public columnsOffset: number;
    public rowsOffset: number;
    public stringResourcesOffset: number;
    public userOffset: number;

    public read(reader: BufferReader): void {
        this.magicValue = reader.readInt32();
        this.hashValue = reader.readInt32();
        this.modifiedTime = reader.readInt64();
        this.tableInfoOffset = reader.readInt64();
        this.columnsOffset = reader.readInt64();
        this.rowsOffset = reader.readInt64();
        this.stringResourcesOffset = reader.readInt64();
        this.userOffset = reader.readInt64();
    }
}

class TableInfo {
    public tableName: number;
    public categoryName: number;
    public columnCount: number;
    public rowCount: number;

    public read(reader: BufferReader): void {
        this.tableName = reader.readInt32();
        this.categoryName = reader.readInt32();
        this.columnCount = reader.readInt32();
        this.rowCount = reader.readInt32();
    }
}

class ColumnInfo {
    public columnName: number;
    public dataType: number;
    public isKey: number;

    public read(reader: BufferReader): void {
        this.columnName = reader.readInt32();
        this.dataType = reader.readInt32();
        this.isKey = reader.readInt32();
    }
}

class FileHeader {
    public static defaultMagicValue: number = 0x04000000;

    public magicValue; number;
    public revision: number;
    public typesHashValue: number;
    public tablesHashValue: number;
    public tags: number;
    public reserved: number;
    public tableCount: number;
    public name: number;
    public indexOffset: number;
    public tablesOffset: number;
    public stringResourcesOffset: number;

    public read(reader: BufferReader): void {
        this.magicValue = reader.readInt32();
        this.revision = reader.readInt32();
        this.typesHashValue = reader.readInt32();
        this.tablesHashValue = reader.readInt32();
        this.tags = reader.readInt32();
        this.reserved = reader.readInt32();
        this.tableCount = reader.readInt32();
        this.name = reader.readInt32();
        this.indexOffset = reader.readInt64();
        this.tablesOffset = reader.readInt64();
        this.stringResourcesOffset = reader.readInt64();
    }
}

class TableIndex {
    public tableName: number;
    public dummy: number;
    public offset: number;

    public read(reader: BufferReader): void {
        this.tableName = reader.readInt32();
        this.dummy = reader.readInt32();
        this.offset = reader.readInt64();
    }
}

class CremaTable {

}

class CremaColumn {

}

class CremaRow {

}

export interface IDataSet {
    tables?: { [key: string]: ITable; };
    revision?: string;
    name?: string;
    typesHashValue?: string;
    tablesHashValue?: string;
    tags?: string;
}

export interface ITable extends CremaTable {
    dataSet?: IDataSet;
    name?: string;
    category?: string;
    index?: number;
    version?: number;
    hashValue?: string;
    keys?: IColumn[];
    columns?: { [key: number]: IColumn; };
    columnsByName?: { [key: string]: IColumn; };
    rows?: IRow[];
}

export interface IColumn extends CremaColumn {
    table?: ITable;
    name?: string;
    dataType?: string;
    isKey?: boolean;
    index?: number;
}

export interface IRow extends CremaRow {
    table?: ITable;
    getValue(index: number): string | number | boolean | Date;
    hasValue(index: number): boolean;
    toBoolean(index: number): boolean;
    toString(index: number): string;
    toSingle(index: number): number;
    toDouble(index: number): number;
    toInt8(index: number): number;
    toUInt8(index: number): number;
    toInt16(index: number): number;
    toUInt16(index: number): number;
    toInt32(index: number): number;
    toUInt32(index: number): number;
    toInt64(index: number): number;
    toUInt64(index: number): number;
    toDateTime(index: number): Date;
    toDuration(index: number): number;

    getValueByString(columnName: string): string | number | boolean | Date;
    hasValueByString(columnName: string): boolean;
    toBooleanByString(columnName: string): boolean;
    toStringByString(columnName: string): string;
    toSingleByString(columnName: string): number;
    toDoubleByString(columnName: string): number;
    toInt8ByString(columnName: string): number;
    toUInt8ByString(columnName: string): number;
    toInt16ByString(columnName: string): number;
    toUInt16ByString(columnName: string): number;
    toInt32ByString(columnName: string): number;
    toUInt32ByString(columnName: string): number;
    toInt64ByString(columnName: string): number;
    toUInt64ByString(columnName: string): number;
    toDateTimeByString(columnName: string): Date;
    toDurationByString(columnName: string): number;
}

class BinaryReader extends CremaReader {
    private _tableIndexes: Array<TableIndex>;
    private _tables: Array<BinaryTable>;
    private _t: { [key: string]: ITable; };
    private _version: number;
    private _revision: string;
    private _name: string;
    private _typesHashValue: string;
    private _tablesHashValue: string;
    private _tags: string;

    public read(reader: BufferReader): void {
        let fileHeader: FileHeader = new FileHeader();

        fileHeader.read(reader);
        this._tableIndexes = new Array<TableIndex>(fileHeader.tableCount);
        this._version = fileHeader.magicValue;

        for (let i: number = 0; i < fileHeader.tableCount; i++) {
            let item: TableIndex = new TableIndex();
            item.read(reader);
            this._tableIndexes[i] = item;
        }

        reader.seek(fileHeader.stringResourcesOffset, SeekOrigin.Begin);
        StringResource.read(reader);

        this._name = StringResource.getString(fileHeader.name);
		this._revision = StringResource.getString(fileHeader.revision);
        this._tables = new Array<BinaryTable>(this._tableIndexes.length);
        this._typesHashValue = StringResource.getString(fileHeader.typesHashValue);
        this._tablesHashValue = StringResource.getString(fileHeader.tablesHashValue);
        this._tags = StringResource.getString(fileHeader.tags);

        for (let i: number = 0; i < this._tableIndexes.length; i++) {
            let table: BinaryTable = this.readTable(reader, this._tableIndexes[i].offset);
            this._tables[i] = table;
        }
    }

    public get tables(): { [key: string]: ITable; } {
        if (this._t == null) {
            this._t = {};
            this._tables.forEach((v, i, a) => {
                this._t[v.name] = v;
            });
        }
        return this._t;
    }

    public get version(): number {
        return this._version;
    }

    public get revision(): string {
        return this._revision;
    }

    public get name(): string {
        return this._name;
    }

    public get typesHashValue(): string {
        return this._typesHashValue;
    }

    public get tablesHashValue(): string {
        return this._tablesHashValue;
    }

    public get tags(): string {
        return this._tags;
    }

    private readTable(reader: BufferReader, offset: number): BinaryTable {
        let tableHeader: TableHeader = new TableHeader();
        let tableInfo: TableInfo = new TableInfo();

        reader.seek(offset, SeekOrigin.Begin);
        tableHeader.read(reader);

        reader.seek(tableHeader.tableInfoOffset + offset, SeekOrigin.Begin);
        tableInfo.read(reader);

        let table: BinaryTable = new BinaryTable(this, tableHeader, tableInfo);

        reader.seek(tableHeader.stringResourcesOffset + offset, SeekOrigin.Begin);
        StringResource.read(reader);
        table.hashValue = StringResource.getString(tableHeader.hashValue);

        reader.seek(tableHeader.columnsOffset + offset, SeekOrigin.Begin);
        table.readColumns(reader, tableInfo);

        reader.seek(tableHeader.rowsOffset + offset, SeekOrigin.Begin);
        table.readRows(reader, tableInfo);

        return table;
    }
}

class BinaryTable extends CremaTable {

    private _dataSet: BinaryReader;
    private _name: string;
    private _category: string;
    private _version: number;
    private _hashValue: string;
    private _keys: BinaryColumn[];
    private _columns: { [key: number]: BinaryColumn; };
    private _columnsByName: { [key: string]: BinaryColumn; };
    private _rows: BinaryRow[];

    constructor(dataSet: BinaryReader, tableHeader: TableHeader, tableInfo: TableInfo) {
        super();
        this._dataSet = dataSet;
        this._name = StringResource.getString(tableInfo.tableName);
        this._category = StringResource.getString(tableInfo.categoryName);
        this._version = tableHeader.magicValue;
    }

    public get dataSet(): IDataSet {
        return this._dataSet;
    }

    public get name(): string {
        return this._name;
    }

    public get category(): string {
        return this._category;
    }

    public get version(): number {
        return this._version;
    }

    public get hashValue(): string {
        return this._hashValue;
    }

    public set hashValue(value) {
        this._hashValue = value;
    }

    public get columns(): { [key: number]: BinaryColumn; } {
        return this._columns;
    }

    public get columnsByName(): { [key: string]: BinaryColumn; } {
        return this._columnsByName;
    }

    public get rows(): IRow[] {
        return this._rows;
    }

    public readColumns(reader: BufferReader, tableInfo: TableInfo): void {
        let keys: Array<BinaryColumn> = new Array<BinaryColumn>();
        let columns: { [key: number]: BinaryColumn; } = {};
        let columnsByName: { [key: string]: BinaryColumn; } = {};

        for (let i: number = 0; i < tableInfo.columnCount; i++) {
            let columnInfo: ColumnInfo = new ColumnInfo();
            columnInfo.read(reader);

            let columnName: string = StringResource.getString(columnInfo.columnName);
            let typeName: string = StringResource.getString(columnInfo.dataType);
            let isKey: boolean = columnInfo.isKey === 0 ? false : true;

            let column: BinaryColumn = new BinaryColumn(this, columnName, typeName, isKey, i);

            columns[i] = column;
            columnsByName[columnName] = column;
            if (isKey === true) {
                keys.push(column);
            }
        }

        this._columns = columns;
        this._columnsByName = columnsByName;
        this._keys = keys;
    }

    public readRows(reader: BufferReader, tableInfo: TableInfo): void {
        this._rows = new Array<BinaryRow>(tableInfo.rowCount);
        for (let i: number = 0; i < tableInfo.rowCount; i++) {
            let length: number = reader.readInt32();
            let fields: Buffer = reader.readBytes(length);
            let dataRow: BinaryRow = new BinaryRow(this, fields);
            this._rows[i] = dataRow;
        }
    }
}

class BinaryColumn extends CremaColumn {
    private _table: BinaryTable;
    private _columnName: string;
    private _type: string;
    private _isKey: boolean;
    private _index: number;

    constructor(table: BinaryTable, columnName: string, typeName: string, isKey: boolean, index: number) {
        super();
        this._table = table;
        this._columnName = columnName;
        this._type = this.nameToType(typeName);
        this._isKey = isKey;
        this._index = index;
    }

    public get table(): BinaryTable {
        return this._table;
    }

    public get name(): string {
        return this._columnName;
    }

    public get dataType(): string {
        return this._type;
    }

    public get isKey(): boolean {
        return this._isKey;
    }

    public get index(): number {
        return this._index;
    }

    private nameToType(typeName: string): string {
        if (typeName === "boolean") {
            return "boolean";
        } else if (typeName === "string") {
            return "string";
        } else if (typeName === "float") {
            return "float";
        } else if (typeName === "double") {
            return "double";
        } else if (typeName === "int8") {
            return "int8";
        } else if (typeName === "uint8") {
            return "uint8";
        } else if (typeName === "int16") {
            return "int16";
        } else if (typeName === "uint16") {
            return "uint16";
        } else if (typeName === "int32") {
            return "int32";
        } else if (typeName === "uint32") {
            return "uint32";
        } else if (typeName === "int64") {
            return "int64";
        } else if (typeName === "uint64") {
            return "uint64";
        } else if (typeName === "datetime") {
            return "int64";
        } else if (typeName === "duration") {
            return "int64";
		} else if (typeName === "guid") {
            return "string";
        }
        return "int64";
    }
}

class BinaryRow extends CremaRow {
    private _table: BinaryTable;
    private _fieldbytes: Buffer;

    public constructor(table: BinaryTable, fieldbytes: Buffer) {
        super();
        this._table = table;
        this._fieldbytes = fieldbytes;
    }

    public get table(): BinaryTable {
        return this._table;
    }

    public getValueByColumn(column: BinaryColumn): string | number | boolean {
        var offset: number = this._fieldbytes.readInt32LE(4 * column.index);

        if (column.dataType === "string") {
            if (offset === 0) {
                return "";
            }
            var id: number = this._fieldbytes.readInt32LE(offset);
            return StringResource.getString(id);
        } else {
            if (offset === 0) {
                return null;
            }
            if (column.dataType === "boolean") {
                return this._fieldbytes.readInt8(offset) === 0 ? false : true;
			} else if (column.dataType === "float") {
                return this._fieldbytes.readFloatLE(offset);
            } else if (column.dataType === "double") {
                return this._fieldbytes.readDoubleLE(offset);
			} else if (column.dataType === "int8") {
                return this._fieldbytes.readInt8(offset);
            } else if (column.dataType === "uint8") {
                return this._fieldbytes.readUInt8(offset);
            } else if (column.dataType === "int16") {
                return this._fieldbytes.readInt16LE(offset);
            } else if (column.dataType === "uint16") {
                return this._fieldbytes.readUInt16LE(offset);
            } else if (column.dataType === "int32") {
                return this._fieldbytes.readInt32LE(offset);
            } else if (column.dataType === "uint32") {
                return this._fieldbytes.readUInt32LE(offset);
            } else if (column.dataType === "int64") {
                return this._fieldbytes.readIntLE(offset, 8);
            } else if (column.dataType === "uint64") {
                return this._fieldbytes.readUIntLE(offset, 8);
            }
            throw new Error(column.dataType);
        }
    }

    public getValue(index: number): string | number | boolean {
        let column: BinaryColumn = this.table.columns[index];
        return this.getValueByColumn(column);
    }

    public getValueByString(columnName: string): string | number | boolean {
        let column: BinaryColumn = this.table.columnsByName[columnName];
        return this.getValueByColumn(column);
    }

    public hasValueByColumn(column: BinaryColumn): boolean {
        let offset: number = this._fieldbytes.readInt32LE(4 * column.index);
        return offset !== 0;
    }

    public hasValue(index: number): boolean {
        let column: BinaryColumn = this._table.columns[index];
        return this.hasValueByColumn(column);
    }

    public hasValueByString(columnName: string): boolean {
        let column: BinaryColumn = this._table.columnsByName[columnName];
        return this.hasValueByColumn(column);
    }

    public toBoolean(index: number): boolean {
        return <boolean>this.getValue(index);
    }

    public toString(index: number): string {
        return <string>this.getValue(index);
    }

    public toSingle(index: number): number {
        return <number>this.getValue(index);
    }

    public toDouble(index: number): number {
        return <number>this.getValue(index);
    }

    public toInt8(index: number): number {
        return <number>this.getValue(index);
    }

    public toUInt8(index: number): number {
        return <number>this.getValue(index);
    }

    public toInt16(index: number): number {
        return <number>this.getValue(index);
    }

    public toUInt16(index: number): number {
        return <number>this.getValue(index);
    }

    public toInt32(index: number): number {
        return <number>this.getValue(index);
    }

    public toUInt32(index: number): number {
        return <number>this.getValue(index);
    }

    public toInt64(index: number): number {
        return <number>this.getValue(index);
    }

    public toUInt64(index: number): number {
        return <number>this.getValue(index);
    }

    public toDateTime(index: number): Date {
        return new Date(<number>this.getValue(index) * 1000);
    }

    public toDuration(index: number): number {
        return <number>this.getValue(index);
    }

    public toBooleanByString(columnName: string): boolean {
        return <boolean>this.getValueByString(columnName);
    }

    public toStringByString(columnName: string): string {
        return <string>this.getValueByString(columnName);
    }

    public toSingleByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toDoubleByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toInt8ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toUInt8ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toInt16ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toUInt16ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toInt32ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toUInt32ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toInt64ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toUInt64ByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }

    public toDateTimeByString(columnName: string): Date {
        return new Date(<number>this.getValueByString(columnName) * 1000);
    }

    public toDurationByString(columnName: string): number {
        return <number>this.getValueByString(columnName);
    }
}

class BufferReader {
    private pos: number;
    private buffer: Buffer;

    constructor(buffer: Buffer) {
        this.pos = 0;
        this.buffer = buffer;
    }

    public readBytes(length: number): Buffer {
        let buf: Buffer = new Buffer(length);
        this.buffer.copy(buf, 0, this.pos, this.pos + length);
        this.pos += length;
        return buf;
    }

    public readInt8(): number {
        let value: number = this.buffer.readInt8(this.pos);
        this.pos += 1;
        return value;
    }

    public readUInt8(): number {
        let value: number = this.buffer.readUInt8(this.pos);
        this.pos += 1;
        return value;
    }

    public readInt32(): number {
        let value: number = this.buffer.readInt32LE(this.pos);
        this.pos += 4;
        return value;
    }

    public readUInt32(): number {
        let value: number = this.buffer.readUInt32LE(this.pos);
        this.pos += 4;
        return value;
    }

    public readInt64(): number {
        let value: number = this.buffer.readIntLE(this.pos, 8);
        this.pos += 8;
        return value;
    }

    public readUInt64(): number {
        let value: number = this.buffer.readUIntLE(this.pos, 8);
        this.pos += 8;
        return value;
    }

    public get position(): number {
        return this.pos;
    }

    public seek(offset: number, origin: SeekOrigin): void {
        if (origin === SeekOrigin.Begin) {
            this.pos = offset;
        } else if (origin === SeekOrigin.Current) {
            this.pos += offset;
        } else if (origin = SeekOrigin.End) {
            this.pos = this.buffer.length + offset;
        }
    }

    public readString(length: number): string {
        let buf: Buffer = new Buffer(length);
        for (let i: number = 0; i < length; i++) {
            buf[i] = this.buffer[this.pos + i];
        }

        let text: string = buf.toString("utf8");
        let l: number = text.length;
        this.pos += length;
        return text;
    }
}
