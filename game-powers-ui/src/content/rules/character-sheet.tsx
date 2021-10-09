import { ForwardedRef, forwardRef, Fragment, ReactNode } from 'react';

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
}

.stat-word {
	font-family: "Lato", sans-serif;
	font-size: 28px;
	font-weight: bold;
}

.stat-abbreviation {
	font-family: "Lato", sans-serif;
	font-size: 28px;
	font-weight: bold;
	text-transform: uppercase;
}

.stat-annotation {
	font-family: "Lato", sans-serif;
	font-size: 16px;
}

.stat-label {
	font-family: "Lato", sans-serif;
	font-size: 14px;
	font-weight: bold;
	text-transform: uppercase;
}
.stat-label.large {
	font-size: 20px;
	font-weight: bold;
	text-transform: initial;
}
.label {
	font-family: "Lato", sans-serif;
	font-size: 14px;
	font-weight: bold;
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
	const x = left !== undefined ? left : right !== undefined ? right - width : (undefined as never);
	const y = top !== undefined ? top : bottom !== undefined ? bottom - height : (undefined as never);

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

function UnderlineBox({ children, label, ...props }: BoxProps & { label: string }) {
	return (
		<HiddenBox {...props} strokeWidth={2}>
			{({ width, height }) => (
				<>
					<line x1={-1} x2={width + 2} y1={height + 1} y2={height + 1} strokeWidth={2} stroke={black} />
					<text className="label" y={height + 7} {...text.y.hanging} fill={black}>
						{label}
					</text>
					{typeof children === 'function' ? children({ width, height }) : children}
				</>
			)}
		</HiddenBox>
	);
}

function LabelledBox({ children, label, ...props }: BoxProps & { label: string }) {
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
	modifiers,
	children,
	mode = 'box',
}: {
	firstLabel: string;
	x: number;
	y: number;
	width: number;
	height: number;
	modifiers: Array<string | string[]>;
	children?: BoxProps['children'];
	mode?: 'box' | 'circle';
} & ({ firstBoxWidth: number; mode?: 'box' } | { firstBoxWidth?: undefined; mode: 'circle' })) {
	const modWidth = 54 - Math.max(0, modifiers.length - 4);
	return (
		<HiddenBox left={x} top={height - 50} outerWidth={width} outerHeight={50}>
			<rect x="16" width={width - 16} height={50} fill={black} />
			<text className="stat-label" y="1" x="2" {...text.y.base}>
				{firstLabel}
			</text>
			{mode === 'box' ? (
				<rect y="5" width={firstBoxWidth} height="40" fill="white" strokeWidth="2" stroke={black} />
			) : (
				<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
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
								y="1"
								dy={14 * (lineIndex - lines.length + 1)}
								className="stat-label"
								{...text.x.center}
								{...text.y.base}>
								{line}
							</text>
						))}
						<rect x={mx} y="7" width="47" height="36" fill="white" strokeWidth="0" />
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
}: {
	x: number;
	y: number;
	width: number;
	height: number;
	abbreviation: string;
	name: string;
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
		</HiddenBox>
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
						DnD: ME
					</text>
					<BorderBox right={1487} top={52} innerWidth={462} innerHeight={handwritingHeight}>
						{({ height }) => (
							// {/* <rect x="1025" y="52" width="462" height="38" strokeWidth="2" stroke={black} fill="white" /> */}
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
							firstLabel="Score"
							firstBoxWidth={65}
							x={0}
							y={0}
							width={498}
							height={68}
							modifiers={['Base', 'Armor', 'Item', 'Misc']}>
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
							Score
						</text>
						<text className="stat-label" y="1" x="388" {...text.x.center} {...text.y.base}>
							Score + 1 / 2 LVL
						</text>
					</g>
					<g transform="translate(16 50)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar width={482} height={48} x={0} y={0} abbreviation="STR" name="Strength" />
						<AttributeBar width={482} height={48} x={0} y={51} abbreviation="CON" name="Constitution" />
					</g>
					<g transform="translate(16 170)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar width={482} height={48} x={0} y={0} abbreviation="DEX" name="Dexterity" />
						<AttributeBar width={482} height={48} x={0} y={51} abbreviation="INT" name="Intelligence" />
					</g>
					<g transform="translate(16 290)">
						<path transform="translate(0 23)" d="M190 0v53H424L539 0z" fill="#888888" />
						<AttributeBar width={482} height={48} x={0} y={0} abbreviation="WIS" name="Wisdom" />
						<AttributeBar width={482} height={48} x={0} y={51} abbreviation="CHA" name="Charisma" />
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
						<ModifierBar mode="circle" firstLabel="" x={0} y={0} height={50} width={498} modifiers={acModifiers}>
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
									<text className="stat-abbreviation" y="31" x={width + 47 / 2} {...text.x.center} fill={black}>
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
									<text className="stat-abbreviation" y="31" x={width + 47 / 2} {...text.x.center} fill={black}>
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
									<text className="stat-abbreviation" y="31" x={width + 47 / 2} {...text.x.center} fill={black}>
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
					<g transform="translate(0 140)">
						<rect x="0" y="0" width="498" height="111" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Current Hit Points
						</text>
						<text className="stat-label" y="2" x="496" {...text.x.right} {...text.y.hanging}>
							Current Surge Uses
						</text>
					</g>
					<g transform="translate(0 251)">
						<rect width="498" height="26" fill={black} />
						<text className="stat-label large" y="13" x="200" {...text.x.center} {...text.y.middle} fill="white">
							Second Wind 1/Encounter
						</text>
						<text className="stat-label large" y="13" x="400" {...text.x.right} {...text.y.middle} fill="white">
							Used
						</text>
						<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					</g>
					<g transform="translate(0 277)">
						<rect x="0" y="0" width="498" height="67" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Temporary Points
						</text>
					</g>
					<g transform="translate(0 344)">
						<rect width="498" height="26" fill={black} />
						<text className="stat-label large" y="13" x="200" {...text.x.center} {...text.y.middle} fill="white">
							Death Saving Throw Failures
						</text>
						<rect x="381" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
						<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
						<rect x="427" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					</g>
					<g transform="translate(0 370)">
						<rect x="0" y="0" width="498" height="50" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Saving Throw Mods
						</text>
					</g>
					<g transform="translate(0 420)">
						<rect x="0" y="0" width="498" height="62" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Resistances
						</text>
					</g>
					<g transform="translate(0 482)">
						<rect x="0" y="0" width="498" height="62" fill="white" strokeWidth="2" stroke={black} />
						<text className="stat-label" y="2" x="2" textAnchor="start" {...text.y.hanging}>
							Current Conditions and Effects
						</text>
					</g>
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

				<g id="attack-workspace" transform="translate(1014 215)">
					<MiniBanner width={498} height={34} label="Attack Workspace" />
					<g transform="translate(0 36)" id="attack-workspace-ability">
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
					</g>
					<use transform="translate(0 170)" href="#attack-workspace-ability" />
					<use transform="translate(0 340)" href="#attack-workspace-ability" />
				</g>

				<g id="basic-attacks" transform="translate(1014 762)">
					<MiniBanner width={498} height={34} label="Basic Attacks" />
					<g transform="translate(0 36)" id="attack-workspace-ability">
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
					</g>
					<g transform="translate(0 50)" id="basic-attack">
						<rect y="0" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
						<rect y="0" x="97" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
						<text className="label" y="32" x="79" {...text.x.center} {...text.y.base}>
							vs
						</text>
						<line x1="171" x2="368" y1="42" y2="42" strokeWidth="2" stroke={black} />
						<line x1="382" x2="498" y1="42" y2="42" strokeWidth="2" stroke={black} />
					</g>
					<use href="#basic-attack" transform="translate(0 50)" />
					<use href="#basic-attack" transform="translate(0 100)" />
					<use href="#basic-attack" transform="translate(0 150)" />
				</g>

				<g id="languages" transform="translate(0 1310)">
					<MiniBanner width={498} height={34} label="Languages Known" />
					<g transform="translate(0 34)" id="attack-workspace-ability">
						<line x1="0" x2="498" y1="36" y2="36" strokeWidth="2" stroke={black} />
					</g>
					<g transform="translate(0 70)" id="attack-workspace-ability">
						<line x1="0" x2="498" y1="36" y2="36" strokeWidth="2" stroke={black} />
					</g>
				</g>
			</svg>
		);
	}
);
