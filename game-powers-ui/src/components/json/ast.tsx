import classNames from 'classnames';
import { Token, TokenByType } from 'json-to-ast';
import React, { useReducer } from 'react';

type JsonKeyPath = Array<string | number>;

function containsArray<T>(haystack: T[][], needle: T[]) {
	const matches = haystack.filter((e) => needle.every((v, i) => e[i] === v));
	return matches.some((m) => m.length === needle.length) ? 'exact' : matches.length > 0 ? 'partial' : false;
}

const tokenComponents: {
	[K in Token['type']]: React.FunctionComponent<{
		data: TokenByType[K];
		keyPath: JsonKeyPath;
		label?: string;
		highlight: JsonKeyPath[];
	}>;
} = {
	Object: RenderObject,
	Literal: RenderLiteral,
	Array: RenderArray,
};

export function AstViewer({
	data,
	label,
	keyPath,
	highlight,
}: {
	data: Token;
	label?: string;
	keyPath?: JsonKeyPath;
	highlight: JsonKeyPath[];
}) {
	const Component = tokenComponents[data.type] as React.FunctionComponent<{
		data: Token;
		label?: string;
		highlight: JsonKeyPath[];
		keyPath: JsonKeyPath;
	}>;
	if (!Component) return null;
	return <Component data={data} label={label} keyPath={keyPath || ['$']} highlight={highlight} />;
}

function Dot() {
	return (
		<svg className="inline w-4 h-4 mb-1" viewBox="-30 -30 60 60">
			<circle r="10" fill="currentcolor" />
		</svg>
	);
}

function Down() {
	return (
		<svg className="inline w-4 h-4 mb-1" viewBox="-30 -30 60 60">
			<path fill="currentcolor" d="M-20-10L0 20L20-10z" />
		</svg>
	);
}

function Right() {
	return (
		<svg className="inline w-4 h-4 mb-1" viewBox="-30 -30 60 60">
			<path fill="currentcolor" d="M-10-20L20 0L-10 20z" />
		</svg>
	);
}

function Label({
	children,
	expanded,
	highlight,
	onClick,
}: {
	children?: React.ReactChild;
	expanded?: boolean;
	highlight?: boolean;
	onClick?: () => void;
}) {
	return children ? (
		<button
			type="button"
			onClick={onClick}
			className={classNames('mr-2 text-blue-700', { 'bg-orange-dark': highlight })}>
			{expanded === undefined ? <Dot /> : expanded ? <Down /> : <Right />} {children}:{' '}
		</button>
	) : null;
}

function RenderObject({
	data,
	label,
	keyPath,
	highlight,
}: {
	data: TokenByType['Object'];
	label?: string;
	keyPath: JsonKeyPath;
	highlight: JsonKeyPath[];
}) {
	const [expanded, toggleExpanded] = useReducer((prev) => !prev, false);
	const showExpanded = !label || expanded;
	const match = containsArray(highlight, keyPath);
	return (
		<>
			<Label expanded={expanded} onClick={toggleExpanded} highlight={expanded ? match === 'exact' : match !== false}>
				{label}
			</Label>
			<span className="text-green-dark">
				{showExpanded ? '' : '{ '}
				<ul
					className={classNames({
						'ml-4': expanded,
						inline: !showExpanded,
					})}>
					{data.children.map((p, i) => (
						<li
							key={p.key.value}
							className={classNames({
								'inline pr-1': !showExpanded,
							})}>
							{showExpanded ? (
								<AstViewer
									data={p.value}
									label={p.key.value}
									keyPath={[...keyPath, p.key.value]}
									highlight={highlight}
								/>
							) : (
								p.key.value
							)}
							{showExpanded || i >= data.children.length - 1 ? '' : ','}
						</li>
					))}
				</ul>
				{showExpanded ? '' : '}'}
			</span>
		</>
	);
}

function RenderLiteral({
	data,
	label,
	keyPath,
	highlight,
}: {
	data: TokenByType['Literal'];
	label?: string;
	keyPath: JsonKeyPath;
	highlight: JsonKeyPath[];
}) {
	const match = containsArray(highlight, keyPath);
	return (
		<>
			<Label highlight={match !== false}>{label}</Label>
			<span className="text-brown-dark">{data.raw}</span>
		</>
	);
}

function RenderArray({
	data,
	label,
	keyPath,
	highlight,
}: {
	data: TokenByType['Array'];
	label?: string;
	keyPath: JsonKeyPath;
	highlight: JsonKeyPath[];
}) {
	const [expanded, toggleExpanded] = useReducer((prev) => !prev, false);
	const showExpanded = !label || expanded;
	const match = containsArray(highlight, keyPath);
	return (
		<>
			<Label expanded={expanded} onClick={toggleExpanded} highlight={expanded ? match === 'exact' : match !== false}>
				{label}
			</Label>
			<span className="text-green-dark">
				{showExpanded ? '' : '[ '}
				<ol
					className={classNames({
						'ml-4': expanded,
						inline: !showExpanded,
					})}>
					{data.children.map((v, i) => (
						<li
							// eslint-disable-next-line react/no-array-index-key
							key={i}
							className={classNames({
								'inline pr-1': !showExpanded,
							})}>
							{showExpanded ? <AstViewer data={v} label={`${i}`} keyPath={[...keyPath, i]} highlight={highlight} /> : i}
							{showExpanded || i >= data.children.length - 1 ? '' : ','}
						</li>
					))}
				</ol>
				{showExpanded ? '' : ']'}
			</span>
		</>
	);
}
