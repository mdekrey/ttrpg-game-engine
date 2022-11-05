import { ForwardedRef, forwardRef, Fragment, ReactNode, useEffect, useRef, useState } from 'react';
import { ensureSign } from './ensureSign';
import { splitText, svgTextElementToMeasure } from './splitText';

const black = '#333333';
const sharedCss = `
.title {
	font-family: "Source Serif Pro", serif;
	font-size: 64px;
	font-weight: bold;
}
.title.small {
	font-size: 48px;
}

.section-title {
	font-family: "Lato", sans-serif;
	font-size: 20px;
	font-weight: bold;
	text-transform: uppercase;
    fill: white;
}

.stat-word {
	font-family: "Lato", sans-serif;
	font-size: 28px;
	font-weight: bold;
    fill: white;
}

.stat-abbreviation {
	font-family: "Lato", sans-serif;
	font-size: 28px;
	font-weight: bold;
	text-transform: uppercase;
    fill: white;
}

.stat-abbreviation.black {
    fill: ${black};
}

.stat-annotation {
	font-family: "Lato", sans-serif;
	font-size: 16px;
    fill: white;
}

.stat-label {
	font-family: "Lato", sans-serif;
	font-size: 14px;
	font-weight: bold;
	text-transform: uppercase;
    fill: ${black};
}
.stat-label.large {
	font-size: 20px;
	text-transform: initial;
}
.stat-label.white {
	fill: white;
}
.label {
	font-family: "Lato", sans-serif;
	font-size: 14px;
	font-weight: bold;
    fill:  ${black};
}
.handwriting {
	font-size: 28px;
	font-family: Segoe Print, Lucida Grande, sans-serif;
}
.handwriting-detail {
	font-size: 20px;
	font-family: Segoe Print, Lucida Grande, sans-serif;
}
.handwriting-tiny {
	font-size: 16px;
	font-family: Segoe Print, Lucida Grande, sans-serif;
}
.text-measure-hidden {
	fill: transparent;
}
`;
const handwritingHeight = 36;
const text = {
	x: {
		left: { textAnchor: 'start' },
		center: { textAnchor: 'middle' },
		right: { textAnchor: 'end' },
	},
	y: {
		hanging: { dominantBaseline: 'hanging' },
		middle: { dominantBaseline: 'central' },
		base: { dominantBaseline: 'ideographic' },
	},
};

type Either<TOne extends string, TTwo extends string> =
	| ({ [P in TOne]: number } & { [P in TTwo]?: undefined })
	| ({ [P in TTwo]: number } & { [P in TOne]?: undefined });

type BoxProps = Omit<JSX.IntrinsicElements['rect'], 'strokeWidth' | 'width' | 'height' | 'x' | 'y'> &
	Either<'left', 'right'> &
	Either<'top', 'bottom'> &
	Either<'innerHeight', 'outerHeight'> &
	Either<'innerWidth', 'outerWidth'> & {
		children?: ReactNode | ((size: { width: number; height: number }) => JSX.Element);
		strokeWidth?: number;
	};
function Box({
	left,
	right,
	top,
	bottom,
	innerHeight,
	innerWidth,
	outerHeight,
	outerWidth,
	children,
	strokeWidth = 0,
	...props
}: BoxProps) {
	const width =
		innerWidth !== undefined
			? innerWidth + strokeWidth
			: outerWidth !== undefined
			? outerWidth - strokeWidth
			: (undefined as never);
	const height =
		innerHeight !== undefined
			? innerHeight + strokeWidth
			: outerHeight !== undefined
			? outerHeight - strokeWidth
			: (undefined as never);
	const x = left !== undefined ? left + strokeWidth / 2 : right !== undefined ? right - width : (undefined as never);
	const y = top !== undefined ? top + strokeWidth / 2 : bottom !== undefined ? bottom - height : (undefined as never);

	return (
		<g transform={`translate(${x} ${y})`}>
			<rect strokeWidth={strokeWidth} {...props} {...{ width, height }} />
			{children && (
				<g transform={`translate(${strokeWidth / 2} ${strokeWidth / 2})`}>
					{typeof children === 'function'
						? children({ width: width - strokeWidth, height: height - strokeWidth })
						: children}
				</g>
			)}
		</g>
	);
}

function BlackBox({ ...props }: BoxProps) {
	return <Box fill={black} {...props} />;
}

function BorderBox({ ...props }: BoxProps) {
	return <Box strokeWidth={2} stroke={black} fill="white" {...props} />;
}

function HiddenBox({ ...props }: BoxProps) {
	return <Box fill="transparent" stroke="transparent" {...props} />;
}

function UnderlineBox({ children, label, value, ...props }: BoxProps & { label: string; value?: string }) {
	return (
		<HiddenBox {...props} strokeWidth={2}>
			{({ width, height }) => (
				<>
					<line x1={-1} x2={width + 2} y1={height + 1} y2={height + 1} strokeWidth={2} stroke={black} />
					<text className="label" y={height + 7} {...text.y.hanging} fill={black}>
						{label}
					</text>
					{value && (
						<text className="handwriting" y={height - 3} x={2}>
							{value}
						</text>
					)}
					{typeof children === 'function' ? children({ width, height }) : children}
				</>
			)}
		</HiddenBox>
	);
}

function LabelledBox({ children, label, value, ...props }: BoxProps & { label: string; value?: string }) {
	return (
		<HiddenBox {...props} strokeWidth={2}>
			{({ width, height }) => (
				<>
					<rect
						x={-1}
						width={width + 2}
						y={height - handwritingHeight - 1}
						height={handwritingHeight + 2}
						strokeWidth={2}
						stroke={black}
						fill="white"
					/>
					<text className="label" x={width / 2} y={height + 7} {...text.x.center} {...text.y.hanging}>
						{label}
					</text>
					{value && (
						<text className="handwriting" y={height - 3} x={width / 2} {...text.x.center}>
							{value}
						</text>
					)}
					{typeof children === 'function' ? children({ width, height }) : children}
				</>
			)}
		</HiddenBox>
	);
}

function MiniBanner({ label, width, height }: { label: string; width: number; height: number }) {
	return (
		<BlackBox left={0} top={0} outerWidth={498} outerHeight={34}>
			<text className="section-title" y={height / 2} x={width / 2} fill="white" {...text.x.center} {...text.y.middle}>
				{label}
			</text>
		</BlackBox>
	);
}

function ModifierBar({
	firstLabel,
	firstBoxWidth,
	x,
	width,
	height,
	fill,
	modifiers,
	children,
	mode = 'box',
	total,
	modifierValues,
}: {
	firstLabel: string;
	x: number;
	y: number;
	width: number;
	height: number;
	fill?: string;
	modifiers: Array<string | string[]>;
	children?: BoxProps['children'];
	mode?: 'box' | 'circle';
	total?: string;
	modifierValues?: Array<string | undefined | null>;
} & ({ firstBoxWidth: number; mode?: 'box' } | { firstBoxWidth?: undefined; mode: 'circle' })) {
	const modWidth = 54 - Math.max(0, modifiers.length - 4);
	return (
		<HiddenBox left={x} top={height - 50} outerWidth={width} outerHeight={50}>
			<rect x="17" width={width - 16 - 1} height={50 - 1} stroke={black} strokeWidth={2} fill={fill || black} />
			<text className="stat-label" y="0" x="2" {...text.y.base}>
				{firstLabel}
			</text>
			{mode === 'box' && firstBoxWidth ? (
				<>
					<rect x="1" y="5" width={firstBoxWidth - 2} height="40" fill="white" strokeWidth="2" stroke={black} />
					{total !== undefined && (
						<text
							className="handwriting"
							y={5 + 40 / 2}
							x={(firstBoxWidth - 3) / 2 + 1}
							{...text.x.center}
							{...text.y.middle}>
							{total}
						</text>
					)}
				</>
			) : (
				<>
					<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
					{total !== undefined && (
						<text className="handwriting" y="25" x="28" {...text.x.center} {...text.y.middle}>
							{total}
						</text>
					)}
				</>
			)}
			{modifiers.map((modifier, index) => {
				const mx = width - (modifiers.length - index) * modWidth;
				const lines = typeof modifier === 'string' ? [modifier] : modifier;
				return (
					// eslint-disable-next-line react/no-array-index-key
					<Fragment key={index}>
						{lines.map((line, lineIndex) => (
							<text
								// eslint-disable-next-line react/no-array-index-key
								key={lineIndex}
								x={mx}
								dx="23"
								y="0"
								dy={14 * (lineIndex - lines.length + 1)}
								className="stat-label"
								{...text.x.center}
								{...text.y.base}>
								{line}
							</text>
						))}
						<rect x={mx - 1} y="6" width="49" height="38" fill="white" strokeWidth="2" stroke={black} />
						{modifierValues && typeof modifierValues[index] === 'string' && (
							<text
								className="handwriting-detail"
								y={6 + 38 / 2}
								x={mx}
								dx={49 / 2}
								{...text.x.center}
								{...text.y.middle}>
								{modifierValues[index]}
							</text>
						)}
					</Fragment>
				);
			})}
			<HiddenBox
				left={firstBoxWidth || 28 * 2}
				top={5}
				outerWidth={width - (firstBoxWidth || 28 * 2) - modifiers.length * modWidth}
				outerHeight={40}>
				{children}
			</HiddenBox>
		</HiddenBox>
	);
}

function AttributeBar({
	x,
	y,
	width,
	height,
	abbreviation,
	name,
	modifier,
	modifierWithLevel,
}: {
	x: number;
	y: number;
	width: number;
	height: number;
	abbreviation: string;
	name: string;
	modifier?: number;
	modifierWithLevel?: number;
}) {
	return (
		<HiddenBox left={x} top={y} outerWidth={width} outerHeight={height}>
			<rect x="16" width={174} height={48} fill={black} />
			<rect y="4" width={65} height="40" fill="white" strokeWidth="2" stroke={black} />
			<HiddenBox left={65} top={4} outerWidth={174 - 65} outerHeight={40}>
				<text className="stat-abbreviation" y="26" x="62" {...text.x.center} {...text.y.base} fill="white">
					{abbreviation}
				</text>
				<text className="stat-annotation" y="26" x="62" {...text.x.center} {...text.y.hanging} fill="white">
					{name}
				</text>
			</HiddenBox>

			<rect x="350" width="43" height="48" fill={black} />
			<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />

			{typeof modifier === 'number' && (
				<text className="handwriting" x={65 / 2} y={24} {...text.x.center} {...text.y.middle}>
					{ensureSign(modifier)}
				</text>
			)}
			{typeof modifierWithLevel === 'number' && (
				<text className="handwriting" x={353 + 69 / 2} y={24} {...text.x.center} {...text.y.middle}>
					{ensureSign(modifierWithLevel)}
				</text>
			)}
		</HiddenBox>
	);
}

function FreeText({
	x,
	y,
	width,
	height,
	contents,
	maxCols,
}: {
	maxCols: number;
	x: number;
	y: number;
	width: number;
	height: number;
	contents: string[];
}) {
	const [actualTextArrays, updateActualTextArrays] = useState<{ line: string; dx: number; dy: number }[]>(
		contents.map((line) => ({ line, dx: 0, dy: 0 }))
	);
	const measureTextRefRef = useRef<SVGTextElement | null>(null);

	useEffect(() => {
		const elem = measureTextRefRef.current;
		if (!elem) return;
		const measure = svgTextElementToMeasure(elem);
		const contentLines = contents.flatMap((v) => splitText(measure, v, width));
		if (contentLines.length === 0) return;
		const maxWidth = Math.max(...contentLines.map(measure));
		const cols = Math.max(1, Math.min(maxCols, Math.floor(width / maxWidth), contentLines.length));
		const rows = Math.ceil(contentLines.length / cols);
		const rowHeight = height / rows;
		updateActualTextArrays(
			contentLines.map((line, index) => ({
				line,
				dx: (width / cols) * (index % cols),
				dy: (Math.floor(index / cols) + 0.5) * rowHeight,
			}))
		);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	return (
		<g transform={`translate(${x} ${y})`}>
			<text className="handwriting-tiny text-measure-hidden" ref={measureTextRefRef} />
			{actualTextArrays.map(({ line, dx, dy }, index) => (
				<text className="handwriting-tiny" x={dx} y={dy} key={index} {...text.y.middle}>
					{line}
				</text>
			))}
		</g>
	);
}

function TextSection({
	width,
	contents,
	x,
	y,
	height,
	...props
}: JSX.IntrinsicElements['g'] & {
	x: number;
	y: number;
	width: number;
	height: number;
	label: string;
	contents?: string | string[];
}) {
	const textArray = contents === undefined ? [] : typeof contents === 'string' ? [contents] : contents;
	const linePositions = useRef<number[]>([]);
	const [actualTextArrays, updateActualTextArrays] = useState<string[]>([]);
	const measureTextRefRef = useRef<SVGTextElement | null>(null);

	useEffect(() => {
		const elem = measureTextRefRef.current;
		if (!elem) return;
		updateActualTextArrays(textArray.flatMap((v) => splitText(svgTextElementToMeasure(elem), v, width)));
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	return (
		<g>
			<text className="handwriting-detail text-measure-hidden" ref={measureTextRefRef} />
			<RepeatingSection
				x={x}
				y={y}
				width={width}
				height={height}
				{...props}
				heightPerItem={handwritingHeight}
				align={17}
				setLinePositions={(v) => {
					linePositions.current = v;
				}}>
				<line x1={0} x2={width} y1={handwritingHeight} y2={handwritingHeight} strokeWidth="2" stroke={black} />
			</RepeatingSection>
			{actualTextArrays.map(
				(line, index) =>
					linePositions.current.length > index && (
						<text
							className="handwriting-detail"
							x={x}
							y={y + linePositions.current[index]}
							dy={handwritingHeight - 4}
							key={index}>
							{line}
						</text>
					)
			)}
		</g>
	);
}

function RepeatingSection({
	x,
	y,
	height,
	width,
	children,
	align,
	heightPerItem,
	label,
	extraRow,
	extraRowHeight = 0,
	setLinePositions,
	...props
}: Omit<JSX.IntrinsicElements['g'], 'children'> & {
	children: React.ReactNode | ((index: number) => React.ReactNode);
	x: number;
	y: number;
	width: number;
	height: number;
	align?: number;
	label: string;
	heightPerItem: number;
	extraRowHeight?: number;
	extraRow?: JSX.Element;
	setLinePositions?: (yValues: number[]) => void;
}) {
	const bannerHeight = 34;
	const innerPadding = 2;
	const base =
		align === undefined
			? bannerHeight + innerPadding + extraRowHeight
			: Math.ceil((bannerHeight + innerPadding + extraRowHeight + y + align) / heightPerItem) * heightPerItem -
			  (y + align);
	const bodyHeight = height - base;
	const repeating = Math.floor(bodyHeight / heightPerItem);
	// eslint-disable-next-line no-param-reassign
	const linePositions = Array(repeating)
		.fill(0)
		.map((_, i) => i)
		.map((index) => base + heightPerItem * index);
	setLinePositions?.(linePositions);
	return (
		<g transform={`translate(${x} ${y})`} {...props}>
			<MiniBanner width={width} height={bannerHeight} label={label} />
			{extraRow && <g transform={`translate(0 ${base - extraRowHeight})`}>{extraRow}</g>}
			{linePositions.map((yValue, index) => (
				<g transform={`translate(0 ${yValue})`} key={yValue}>
					{typeof children === 'function' ? children(index) : children}
				</g>
			))}
		</g>
	);
}

const acModifiers = [['10 +', '1/2 LVL'], ['Armor /', 'Abil'], 'Class', 'Feat', 'ENH', 'Misc', 'Misc'];
const defenseModifiers = [['10'], ['Abil +', '1/2 LVL'], 'Class', 'Feat', 'ENH', 'Misc', 'Misc'];

export const CharacterSheet = forwardRef(
	({ ...props }: JSX.IntrinsicElements['svg'], ref: ForwardedRef<SVGSVGElement>) => {
		return (
			<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1512 2016" {...props} ref={ref}>
				<defs>
					<link
						{...({ xmlns: 'http://www.w3.org/1999/xhtml' } as any)}
						rel="stylesheet"
						href="https://fonts.googleapis.com/css?family=Lato|Martel|Source+Serif+Pro|IM+Fell+Great+Primer"
						type="text/css"
					/>
					<style>{sharedCss}</style>
				</defs>

				<BlackBox id="title-bar" outerWidth={1512} outerHeight={94} left={0} top={0}>
					<text className="title" y={94 / 2} {...text.y.middle} x="15" fill="white">
						D&amp;D: Mashup
					</text>
					<BorderBox right={1487} top={52} innerWidth={462} innerHeight={handwritingHeight}>
						{({ height }) => (
							<text className="label" x="2" y={height - 2} {...text.y.base}>
								Player Name
							</text>
						)}
					</BorderBox>
					<text className="title small" x="1487" y="5" {...text.x.right} {...text.y.hanging} fill="white">
						Character Sheet
					</text>
				</BlackBox>

				<HiddenBox left={0} top={94} outerWidth={1512} outerHeight={46} id="character-name-bar">
					<UnderlineBox left={0} top={0} outerWidth={490} outerHeight={46} label="Character Name" />
					<LabelledBox left={509} top={0} outerWidth={88} outerHeight={46} label="Level" />
					<UnderlineBox left={613} top={0} outerWidth={230} outerHeight={46} label="Class" />
					<UnderlineBox left={852} top={0} outerWidth={236} outerHeight={46} label="Paragon Path" />
					<UnderlineBox left={1099} top={0} outerWidth={244} outerHeight={46} label="Epic Destiny" />
					<LabelledBox left={1348} top={0} outerWidth={154} outerHeight={46} label="Total XP" />
				</HiddenBox>

				<HiddenBox left={0} top={143} outerWidth={1512} outerHeight={46} id="aesthetics-bar">
					<UnderlineBox left={0} top={0} outerWidth={293} outerHeight={46} label="Race" />
					<UnderlineBox left={306} top={0} outerWidth={79} outerHeight={46} label="Size" />
					<UnderlineBox left={398} top={0} outerWidth={60} outerHeight={46} label="Age" />
					<UnderlineBox left={473} top={0} outerWidth={71} outerHeight={46} label="Height" />
					<UnderlineBox left={558} top={0} outerWidth={71} outerHeight={46} label="Weight" />
					<UnderlineBox left={643} top={0} outerWidth={76} outerHeight={46} label="Alignment" />
					<UnderlineBox left={733} top={0} outerWidth={155} outerHeight={46} label="Pronouns" />
					<UnderlineBox left={901} top={0} outerWidth={158} outerHeight={46} label="Deity" />
					<UnderlineBox
						left={1071}
						top={0}
						outerWidth={435}
						outerHeight={46}
						label="Adventuring Company or Other Affiliations"
					/>
				</HiddenBox>

				<HiddenBox left={0} top={215} outerWidth={498} outerHeight={145} id="movement">
					<MiniBanner width={498} height={34} label="Movement" />
					<HiddenBox left={0} top={34} outerWidth={498} outerHeight={68}>
						<ModifierBar
							firstLabel="Total"
							firstBoxWidth={65}
							x={0}
							y={0}
							width={498}
							height={68}
							modifiers={['Base', 'Armor', 'Item', 'Misc']}
							total={undefined}
							modifierValues={[]}>
							<text className="stat-word" y="31" x="17" fill="white">
								Speed
							</text>
							<text className="stat-annotation" y="31" x="106" fill="white">
								(Squares)
							</text>
						</ModifierBar>
					</HiddenBox>
					<HiddenBox left={0} top={102} outerWidth={498} outerHeight={43}>
						<text className="stat-label" y="1" x="2" {...text.y.hanging}>
							Special Movement
						</text>
					</HiddenBox>
				</HiddenBox>

				<HiddenBox left={0} top={360} outerWidth={498} outerHeight={402} id="ability-scores">
					<MiniBanner width={498} height={34} label="Ability Scores" />
					<g transform="translate(16 50)">
						<text className="stat-label" y="1" x="2" {...text.y.base}>
							Mod
						</text>
						<text className="stat-label" y="1" x="388" {...text.x.center} {...text.y.base}>
							Mod + 1 / 2 LVL
						</text>
					</g>
					<g transform="translate(16 50)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={0}
							abbreviation="STR"
							name="Strength"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={51}
							abbreviation="CON"
							name="Constitution"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
					</g>
					<g transform="translate(16 170)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={0}
							abbreviation="DEX"
							name="Dexterity"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={51}
							abbreviation="INT"
							name="Intelligence"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
					</g>
					<g transform="translate(16 290)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={0}
							abbreviation="WIS"
							name="Wisdom"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
						<AttributeBar
							width={482}
							height={48}
							x={0}
							y={51}
							abbreviation="CHA"
							name="Charisma"
							modifier={undefined}
							modifierWithLevel={undefined}
						/>
					</g>
				</HiddenBox>

				<g id="defenses" transform="translate(507 215)">
					<MiniBanner width={498} height={34} label="Defenses" />
					<g transform="translate(0 52)">
						<text className="stat-label" y="1" x="2" {...text.y.base}>
							Score
						</text>
					</g>
					<g transform="translate(0 67)">
						<ModifierBar
							mode="circle"
							firstLabel=""
							x={0}
							y={0}
							height={50}
							width={498}
							modifiers={acModifiers}
							total={undefined}
							modifierValues={[]}>
							{({ width }) => (
								<text className="stat-abbreviation" y="31" x={width / 2} {...text.x.center} fill="white">
									AC
								</text>
							)}
						</ModifierBar>
						<text className="stat-label" y="74" x="2" {...text.y.base}>
							Conditional Bonuses
						</text>
					</g>

					<g transform="translate(0 203)">
						<ModifierBar mode="circle" firstLabel="" x={0} y={0} height={50} width={498} modifiers={defenseModifiers}>
							{({ width }) => (
								<>
									<text className="stat-abbreviation" y="31" x={width / 2} {...text.x.center} fill="white">
										FORT
									</text>
									<text className="stat-abbreviation black" y="31" x={width + 47 / 2} {...text.x.center}>
										10
									</text>
								</>
							)}
						</ModifierBar>
						<text className="stat-label" y="74" x="2" {...text.y.base}>
							Conditional Bonuses
						</text>
					</g>

					<g transform="translate(0 323)">
						<ModifierBar mode="circle" firstLabel="" x={0} y={0} height={50} width={498} modifiers={defenseModifiers}>
							{({ width }) => (
								<>
									<text className="stat-abbreviation" y="31" x={width / 2} {...text.x.center} fill="white">
										REFL
									</text>
									<text className="stat-abbreviation black" y="31" x={width + 47 / 2} {...text.x.center}>
										10
									</text>
								</>
							)}
						</ModifierBar>
						<text className="stat-label" y="74" x="2" {...text.y.base}>
							Conditional Bonuses
						</text>
					</g>

					<g transform="translate(0 443)">
						<ModifierBar mode="circle" firstLabel="" x={0} y={0} height={50} width={498} modifiers={defenseModifiers}>
							{({ width }) => (
								<>
									<text className="stat-abbreviation" y="31" x={width / 2} {...text.x.center} fill="white">
										WILL
									</text>
									<text className="stat-abbreviation black" y="31" x={width + 47 / 2} {...text.x.center}>
										10
									</text>
								</>
							)}
						</ModifierBar>
						<text className="stat-label" y="74" x="2" {...text.y.base}>
							Conditional Bonuses
						</text>
					</g>
				</g>

				<g id="health" transform="translate(0 762)">
					<MiniBanner width={498} height={34} label="Hit Points" />
					<g transform="translate(0 34)">
						<text className="stat-label large" y="28" x="55" {...text.x.center} {...text.y.base}>
							Max HP
						</text>
						<rect x="2" y="33" width="106" height="59" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="45" x="180" {...text.x.center} {...text.y.base}>
							Bloodied
						</text>
						<rect x="122" y="47" width="116" height="40" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="89" x="180" {...text.x.center} {...text.y.hanging}>
							1/2 HP
						</text>

						<text className="stat-label large" y="28" x="383" {...text.x.center} {...text.y.base}>
							Healing Surges
						</text>
						<text className="stat-label" y="45" x="325" {...text.x.center} {...text.y.base}>
							Surge Value
						</text>
						<rect x="267" y="47" width="116" height="40" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="89" x="325" {...text.x.center} {...text.y.hanging}>
							1/4 HP
						</text>
						<text className="stat-label" y="45" x="446" {...text.x.center} {...text.y.base}>
							Surges/Day
						</text>
						<rect x="394" y="47" width="104" height="40" fill="white" strokeWidth="2" stroke={black} />
					</g>
					<BorderBox left={0} outerWidth={498} top={140} innerHeight={111}>
						{({ width }) => (
							<>
								<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
									Current Hit Points
								</text>
								<text className="stat-label" y="2" x={width - 2} {...text.x.right} {...text.y.hanging}>
									Current Surge Uses
								</text>
							</>
						)}
					</BorderBox>
					<g transform="translate(0 251)">
						<rect width="498" height="26" fill={black} />
						<text className="stat-label large white" y="13" x="200" {...text.x.center} {...text.y.middle}>
							Second Wind 1/Encounter
						</text>
						<text className="stat-label large white" y="13" x="400" {...text.x.right} {...text.y.middle}>
							Used
						</text>
						<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					</g>
					<BorderBox left={0} outerWidth={498} top={277} innerHeight={67}>
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Temporary Points
						</text>
					</BorderBox>
					<g transform="translate(0 344)">
						<rect width="498" height="26" fill={black} />
						<text className="stat-label large white" y="13" x="200" {...text.x.center} {...text.y.middle}>
							Death Saving Throw Failures
						</text>
						<rect x="381" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
						<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
						<rect x="427" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					</g>
					<BorderBox left={0} outerWidth={498} top={370} innerHeight={50}>
						{({ width, height }) => (
							<>
								<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
									Saving Throw Mods
								</text>
								<FreeText maxCols={3} x={10} y={10} width={width - 20} height={height - 10} contents={[]} />
							</>
						)}
					</BorderBox>
					<BorderBox left={0} outerWidth={498} top={420} innerHeight={62}>
						{({ width, height }) => (
							<>
								<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
									Resistances
								</text>
								<FreeText maxCols={3} x={10} y={10} width={width - 20} height={height - 10} contents={[]} />
							</>
						)}
					</BorderBox>
					<BorderBox left={0} outerWidth={498} top={482} innerHeight={62}>
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Current Conditions and Effects
						</text>
					</BorderBox>
				</g>

				<g id="action-points" transform="translate(507 762)">
					<MiniBanner width={498} height={34} label="Action Points" />
					<g transform="translate(0 50)">
						<rect x="16" width="249" height="48" fill={black} />
						<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-word" y="26" x="72" textAnchor="left" {...text.y.middle} fill="white">
							Action Points
						</text>
						<g>
							<text className="stat-label" dy="-2" y="0" x="318" {...text.x.center} {...text.y.hanging}>
								Milestones
							</text>
							<text className="stat-label" dy="-2" y="13" x="318" {...text.x.center} {...text.y.hanging}>
								0
							</text>
							<text className="stat-label" dy="-2" y="26" x="318" {...text.x.center} {...text.y.hanging}>
								1
							</text>
							<text className="stat-label" dy="-2" y="39" x="318" {...text.x.center} {...text.y.hanging}>
								2
							</text>
						</g>
						<g>
							<text className="stat-label" dy="-2" y="0" x="428" {...text.x.center} {...text.y.hanging}>
								Action Points
							</text>
							<text className="stat-label" dy="-2" y="13" x="428" {...text.x.center} {...text.y.hanging}>
								1
							</text>
							<text className="stat-label" dy="-2" y="26" x="428" {...text.x.center} {...text.y.hanging}>
								2
							</text>
							<text className="stat-label" dy="-2" y="39" x="428" {...text.x.center} {...text.y.hanging}>
								3
							</text>
						</g>
						<text className="stat-label" y="50" x="2" textAnchor="start" {...text.y.hanging}>
							Additional Effects for Spending Action Points
						</text>
					</g>
				</g>

				<RepeatingSection
					id="attack-workspace"
					x={1014}
					y={215}
					width={498}
					label="Attack Workspace"
					height={547}
					heightPerItem={170}>
					<text className="stat-label" y="1" x="2" {...text.y.hanging}>
						Ability:
					</text>
					<g transform="translate(0 52)">
						<ModifierBar
							x={0}
							y={0}
							height={50}
							width={498}
							firstLabel="ATK Bonus"
							firstBoxWidth={65}
							modifiers={['1/2 LVL', 'ABIL', 'Class', 'Prof', 'Feat', 'Enh', 'Misc']}
						/>
					</g>
					<g transform="translate(0 118)">
						<ModifierBar
							x={0}
							y={0}
							height={50}
							width={498}
							firstLabel="Damage"
							firstBoxWidth={165}
							modifiers={['ABIL', 'Feat', 'Enh', 'Misc', 'Misc']}
						/>
					</g>
				</RepeatingSection>

				<RepeatingSection
					id="basic-attacks"
					x={1014}
					y={762}
					width={498}
					label="Basic Attacks"
					height={265}
					heightPerItem={50}
					extraRowHeight={16}
					extraRow={
						<>
							<text className="stat-label" y="1" x="31" {...text.x.center} {...text.y.hanging}>
								Attack
							</text>
							<text className="stat-label" y="1" x="128" {...text.x.center} {...text.y.hanging}>
								Defense
							</text>
							<text className="stat-label" y="1" x="269" {...text.x.center} {...text.y.hanging}>
								Weapon or Power
							</text>
							<text className="stat-label" y="1" x="440" {...text.x.center} {...text.y.hanging}>
								Damage
							</text>
						</>
					}>
					<rect y="0" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
					<rect y="0" x="97" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
					<text className="label" y="32" x="79" {...text.x.center} {...text.y.base}>
						vs
					</text>
					<line x1="171" x2="368" y1="42" y2="42" strokeWidth="2" stroke={black} />
					<line x1="382" x2="498" y1="42" y2="42" strokeWidth="2" stroke={black} />
				</RepeatingSection>

				<TextSection key="Race Features" label="Race Features" x={0} y={1315} width={498} height={288} />
				<TextSection label="Class / Path / Destiny Features" x={0} y={1603} width={498} height={396} />

				<RepeatingSection x={1014} y={1027} width={498} height={972} label="Skills" heightPerItem={104}>
					{(i) => (
						<>
							<text className="stat-label" y="1" x="2" {...text.y.hanging}>
								Skill Name:
							</text>
							<text className="handwriting" y="1" dy={4} x="2" dx={14} {...text.y.hanging}>
								{undefined}
							</text>
							<g transform="translate(0 52)">
								<ModifierBar
									firstLabel="Skill Bonus"
									firstBoxWidth={65}
									x={0}
									y={0}
									width={498}
									height={50}
									modifiers={['Ranks', 'Misc', 'Misc']}
								/>
							</g>
						</>
					)}
				</RepeatingSection>

				<TextSection label="Feats" x={507} y={955} width={498} height={1603 - 955} contents={[]} />
				<TextSection
					label="Weapon &amp; Armor Proficiencies"
					x={507}
					y={1603}
					width={498}
					height={1819 - 1603}
					contents={[]}
				/>
				<TextSection label="Languages Known" x={507} y={1819} width={498} height={2016 - 1819} contents={[]} />
			</svg>
		);
	}
);
