declare module 'json-to-ast' {
	export type Location = { line: number; column: number; offset: number };
	export type TokenLocation = { start: Location; end: Location; source: string | null };
	export type ObjectToken = { type: 'Object'; children: PropertyToken[]; loc: TokenLocation };
	export type ArrayToken = { type: 'Array'; children: Token[]; loc: TokenLocation };
	export type IdentifierToken = { type: 'Identifier'; value: string; raw: string; loc: TokenLocation };
	export type LiteralToken = {
		type: 'Literal';
		value: string | number | boolean | null;
		raw: string;
		loc: TokenLocation;
	};
	export type PropertyToken = { type: 'Property'; key: IdentifierToken; value: Token; loc: TokenLocation };
	export type TokenByType = {
		Literal: LiteralToken;
		Identifier: IdentifierToken;
		Property: PropertyToken;
		Array: ArrayToken;
		Object: ObjectToken;
	};
	export type Token = ObjectToken | LiteralToken | ArrayToken;

	declare function parse(json: string, settings?: { loc: boolean; source: string }): Token;
	export = parse;
}
