import { ForwardedRef, forwardRef } from 'react';

const black = '#333333';

export const CharacterSheet = forwardRef(
	({ ...props }: JSX.IntrinsicElements['svg'], ref: ForwardedRef<SVGSVGElement>) => (
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1512 2016" {...props} ref={ref}>
			<defs>
				<link
					{...({ xmlns: 'http://www.w3.org/1999/xhtml' } as any)}
					rel="stylesheet"
					href="https://fonts.googleapis.com/css?family=Lato|Martel|Source+Serif+Pro|IM+Fell+Great+Primer"
					type="text/css"
				/>
				<style>{`
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
        `}</style>
			</defs>

			<g id="title-bar">
				<rect width="1512" height="94" fill={black} />
				<text className="title" y="20" x="15" fill="white" dominantBaseline="hanging">
					DnD: ME
				</text>
				<rect x="1025" y="52" width="462" height="38" strokeWidth="2" stroke={black} fill="white" />
				<text className="label" x="1027" y="88" dominantBaseline="ideographic">
					Player Name
				</text>
				<text className="title small" x="1487" y="5" textAnchor="end" dominantBaseline="hanging" fill="white">
					Character Sheet
				</text>
			</g>

			<g id="character-name-bar" transform="translate(0 94)">
				<g transform="translate(0 0)">
					<line x1="0" x2="490" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Character Name
					</text>
				</g>

				<g transform="translate(509 0)">
					<rect x="0" y="6" width="88" height="38" strokeWidth="2" stroke={black} fill="transparent" />
					<text className="label" y="53" x="44" dominantBaseline="hanging" textAnchor="middle">
						Level
					</text>
				</g>

				<g transform="translate(613 0)">
					<line x1="0" x2="230" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Class
					</text>
				</g>

				<g transform="translate(852 0)">
					<line x1="0" x2="236" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Paragon Path
					</text>
				</g>

				<g transform="translate(1099 0)">
					<line x1="0" x2="244" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Epic Destiny
					</text>
				</g>

				<g transform="translate(1348 0)">
					<rect x="0" y="6" width="154" height="38" strokeWidth="2" stroke={black} fill="transparent" />
					<text className="label" y="53" x="77" dominantBaseline="hanging" textAnchor="middle">
						Total XP
					</text>
				</g>
			</g>

			<g id="aesthetics-bar" transform="translate(0 143)">
				<g transform="translate(0 0)">
					<line x1="0" x2="293" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Race
					</text>
				</g>

				<g transform="translate(306 0)">
					<line x1="0" x2="79" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Size
					</text>
				</g>

				<g transform="translate(398 0)">
					<line x1="0" x2="60" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Age
					</text>
				</g>

				<g transform="translate(473 0)">
					<line x1="0" x2="71" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Height
					</text>
				</g>

				<g transform="translate(558 0)">
					<line x1="0" x2="71" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Weight
					</text>
				</g>

				<g transform="translate(643 0)">
					<line x1="0" x2="76" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Alignment
					</text>
				</g>

				<g transform="translate(733 0)">
					<line x1="0" x2="155" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Pronouns
					</text>
				</g>

				<g transform="translate(901 0)">
					<line x1="0" x2="158" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Deity
					</text>
				</g>

				<g transform="translate(1071 0)">
					<line x1="0" x2="435" y1="44" y2="44" strokeWidth="2" stroke={black} />
					<text className="label" y="53" dominantBaseline="hanging">
						Adventuring Company or Other Affiliations
					</text>
				</g>
			</g>

			<g id="movement" transform="translate(0 215)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Movement
					</text>
				</g>
				<g transform="translate(0 52)">
					<rect x="16" width="482" height="50" fill={black} />
					<text className="stat-label" y="1" x="2" dominantBaseline="ideographic">
						Score
					</text>
					<rect y="5" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-word" y="36" x="82" fill="white">
						Speed
					</text>
					<text className="stat-annotation" y="36" x="174" fill="white">
						(Squares)
					</text>

					<text x="282" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Base
					</text>
					<rect x="282" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="336" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Armor
					</text>
					<rect x="336" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="390" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Item
					</text>
					<rect x="390" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="444" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="444" y="7" width="47" height="36" fill="white" strokeWidth="0" />
				</g>
				<g transform="translate(0 125)">
					<text className="stat-label" y="1" x="2" dominantBaseline="ideographic">
						Special Movement
					</text>
				</g>
			</g>

			<g id="ability-scores" transform="translate(0 360)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Ability Scores
					</text>
				</g>
				<g transform="translate(16 0)">
					<g transform="translate(0 50)">
						<text className="stat-label" y="1" x="2" dominantBaseline="ideographic">
							Score
						</text>
						<text className="stat-label" y="1" x="388" textAnchor="middle" dominantBaseline="ideographic">
							Score + 1 / 2 LVL
						</text>
					</g>
					<g transform="translate(0 50)">
						<rect x="190" y="23" width="234" height="53" fill="#888888" />
						<path transform="translate(0 23)" d="M424 0v53L539 0z" fill="#888888" />
						<g>
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								STR
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Strength
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>

						<g transform="translate(0 51)">
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								CON
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Constitution
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>
					</g>
					<g transform="translate(0 170)">
						<rect x="190" y="23" width="234" height="53" fill="#888888" />
						<path transform="translate(0 23)" d="M424 0v53L539 0z" fill="#888888" />
						<g>
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								DEX
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Dexterity
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>

						<g transform="translate(0 51)">
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								INT
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Intelligence
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>
					</g>
					<g transform="translate(0 290)">
						<rect x="190" y="23" width="234" height="53" fill="#888888" />
						<path transform="translate(0 23)" d="M424 0v53L539 0z" fill="#888888" />
						<g>
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								WIS
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Wisdom
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>

						<g transform="translate(0 51)">
							<rect x="16" width="174" height="48" fill={black} />
							<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
							<text
								className="stat-abbreviation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="ideographic"
								fill="white">
								CHA
							</text>
							<text
								className="stat-annotation"
								y="31"
								x="127"
								textAnchor="middle"
								dominantBaseline="hanging"
								fill="white">
								Charisma
							</text>

							<rect x="350" width="43" height="48" fill={black} />
							<rect x="353" y="4" width="69" height="40" fill="white" strokeWidth="2" stroke={black} />
						</g>
					</g>
				</g>
			</g>

			<g id="defenses" transform="translate(507 215)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Defenses
					</text>
				</g>
				<g transform="translate(0 52)">
					<text className="stat-label" y="1" x="2" dominantBaseline="ideographic">
						Score
					</text>
				</g>
				<g transform="translate(0 67)">
					<rect x="16" width="482" height="50" fill={black} />
					<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-abbreviation" y="36" x="96" textAnchor="middle" fill="white">
						AC
					</text>

					<text
						x="141"
						dx="23"
						y="1"
						dy="-14"
						className="stat-label"
						textAnchor="middle"
						dominantBaseline="ideographic">
						10 +
					</text>
					<text x="141" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						1/2 LVL
					</text>
					<rect x="141" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text
						x="192"
						dx="23"
						y="1"
						dy="-14"
						className="stat-label"
						textAnchor="middle"
						dominantBaseline="ideographic">
						Armor /
					</text>
					<text x="192" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Abil
					</text>
					<rect x="192" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Class
					</text>
					<rect x="243" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Feat
					</text>
					<rect x="294" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						ENH
					</text>
					<rect x="345" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="396" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="447" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text className="stat-label" y="74" x="2" dominantBaseline="ideographic">
						Conditional Bonuses
					</text>
				</g>

				<g transform="translate(0 203)">
					<rect x="16" width="482" height="50" fill={black} />
					<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-abbreviation" y="36" x="96" textAnchor="middle" fill="white">
						FORT
					</text>

					<text
						x="141"
						dx="23"
						y="1"
						dy="-14"
						className="stat-label"
						textAnchor="middle"
						dominantBaseline="ideographic">
						10 +
					</text>
					<text x="141" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						1/2 LVL
					</text>
					<rect x="141" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="192" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Abil
					</text>
					<rect x="192" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Class
					</text>
					<rect x="243" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Feat
					</text>
					<rect x="294" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						ENH
					</text>
					<rect x="345" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="396" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="447" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text className="stat-label" y="74" x="2" dominantBaseline="ideographic">
						Conditional Bonuses
					</text>
				</g>

				<g transform="translate(0 323)">
					<rect x="16" width="482" height="50" fill={black} />
					<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-abbreviation" y="36" x="96" textAnchor="middle" fill="white">
						REFL
					</text>

					<text
						x="141"
						dx="23"
						y="1"
						dy="-14"
						className="stat-label"
						textAnchor="middle"
						dominantBaseline="ideographic">
						10 +
					</text>
					<text x="141" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						1/2 LVL
					</text>
					<rect x="141" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="192" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Abil
					</text>
					<rect x="192" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Class
					</text>
					<rect x="243" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Feat
					</text>
					<rect x="294" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						ENH
					</text>
					<rect x="345" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="396" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="447" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text className="stat-label" y="74" x="2" dominantBaseline="ideographic">
						Conditional Bonuses
					</text>
				</g>

				<g transform="translate(0 443)">
					<rect x="16" width="482" height="50" fill={black} />
					<circle cy="25" cx="28" r="28" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-abbreviation" y="36" x="96" textAnchor="middle" fill="white">
						WILL
					</text>

					<text
						x="141"
						dx="23"
						y="1"
						dy="-14"
						className="stat-label"
						textAnchor="middle"
						dominantBaseline="ideographic">
						10 +
					</text>
					<text x="141" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						1/2 LVL
					</text>
					<rect x="141" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="192" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Abil
					</text>
					<rect x="192" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Class
					</text>
					<rect x="243" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Feat
					</text>
					<rect x="294" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						ENH
					</text>
					<rect x="345" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="396" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
						Misc
					</text>
					<rect x="447" y="7" width="47" height="36" fill="white" strokeWidth="0" />
					<text className="stat-label" y="74" x="2" dominantBaseline="ideographic">
						Conditional Bonuses
					</text>
				</g>
			</g>

			<g id="health" transform="translate(0 762)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Hit Points
					</text>
				</g>
				<g transform="translate(0 34)">
					<text className="stat-label large" y="28" x="55" textAnchor="middle" dominantBaseline="ideographic">
						Max HP
					</text>
					<rect x="2" y="33" width="106" height="59" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="45" x="180" textAnchor="middle" dominantBaseline="ideographic">
						Bloodied
					</text>
					<rect x="122" y="47" width="116" height="40" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="89" x="180" textAnchor="middle" dominantBaseline="hanging">
						1/2 HP
					</text>

					<text className="stat-label large" y="28" x="383" textAnchor="middle" dominantBaseline="ideographic">
						Healing Surges
					</text>
					<text className="stat-label" y="45" x="325" textAnchor="middle" dominantBaseline="ideographic">
						Surge Value
					</text>
					<rect x="267" y="47" width="116" height="40" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="89" x="325" textAnchor="middle" dominantBaseline="hanging">
						1/4 HP
					</text>
					<text className="stat-label" y="45" x="446" textAnchor="middle" dominantBaseline="ideographic">
						Surges/Day
					</text>
					<rect x="394" y="47" width="104" height="40" fill="white" strokeWidth="2" stroke={black} />
				</g>
				<g transform="translate(0 140)">
					<rect x="0" y="0" width="498" height="111" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="2" x="2" textAnchor="start" dominantBaseline="hanging">
						Current Hit Points
					</text>
					<text className="stat-label" y="2" x="496" textAnchor="end" dominantBaseline="hanging">
						Current Surge Uses
					</text>
				</g>
				<g transform="translate(0 251)">
					<rect width="498" height="26" fill={black} />
					<text className="stat-label large" y="13" x="200" textAnchor="middle" dominantBaseline="middle" fill="white">
						Second Wind 1/Encounter
					</text>
					<text className="stat-label large" y="13" x="400" textAnchor="end" dominantBaseline="middle" fill="white">
						Used
					</text>
					<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
				</g>
				<g transform="translate(0 277)">
					<rect x="0" y="0" width="498" height="67" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="2" x="2" textAnchor="start" dominantBaseline="hanging">
						Temporary Points
					</text>
				</g>
				<g transform="translate(0 344)">
					<rect width="498" height="26" fill={black} />
					<text className="stat-label large" y="13" x="200" textAnchor="middle" dominantBaseline="middle" fill="white">
						Death Saving Throw Failures
					</text>
					<rect x="381" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					<rect x="404" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
					<rect x="427" y="3" width="20" height="20" fill="white" strokeWidth="2" stroke={black} />
				</g>
				<g transform="translate(0 370)">
					<rect x="0" y="0" width="498" height="50" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="2" x="2" textAnchor="start" dominantBaseline="hanging">
						Saving Throw Mods
					</text>
				</g>
				<g transform="translate(0 420)">
					<rect x="0" y="0" width="498" height="62" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="2" x="2" textAnchor="start" dominantBaseline="hanging">
						Resistances
					</text>
				</g>
				<g transform="translate(0 482)">
					<rect x="0" y="0" width="498" height="62" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-label" y="2" x="2" textAnchor="start" dominantBaseline="hanging">
						Current Conditions and Effects
					</text>
				</g>
			</g>

			<g id="action-points" transform="translate(507 762)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Action Points
					</text>
				</g>
				<g transform="translate(0 50)">
					<rect x="16" width="249" height="48" fill={black} />
					<rect y="4" width="65" height="40" fill="white" strokeWidth="2" stroke={black} />
					<text className="stat-word" y="26" x="72" textAnchor="left" dominantBaseline="middle" fill="white">
						Action Points
					</text>
					<g>
						<text className="stat-label" dy="-2" y="0" x="318" textAnchor="middle" dominantBaseline="hanging">
							Milestones
						</text>
						<text className="stat-label" dy="-2" y="13" x="318" textAnchor="middle" dominantBaseline="hanging">
							0
						</text>
						<text className="stat-label" dy="-2" y="26" x="318" textAnchor="middle" dominantBaseline="hanging">
							1
						</text>
						<text className="stat-label" dy="-2" y="39" x="318" textAnchor="middle" dominantBaseline="hanging">
							2
						</text>
					</g>
					<g>
						<text className="stat-label" dy="-2" y="0" x="428" textAnchor="middle" dominantBaseline="hanging">
							Action Points
						</text>
						<text className="stat-label" dy="-2" y="13" x="428" textAnchor="middle" dominantBaseline="hanging">
							1
						</text>
						<text className="stat-label" dy="-2" y="26" x="428" textAnchor="middle" dominantBaseline="hanging">
							2
						</text>
						<text className="stat-label" dy="-2" y="39" x="428" textAnchor="middle" dominantBaseline="hanging">
							3
						</text>
					</g>
					<text className="stat-label" y="50" x="2" textAnchor="start" dominantBaseline="hanging">
						Additional Effects for Spending Action Points
					</text>
				</g>
			</g>

			<g id="attack-workspace" transform="translate(1014 215)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Attack Workspace
					</text>
				</g>
				<g transform="translate(0 36)" id="attack-workspace-ability">
					<text className="stat-label" y="1" x="2" dominantBaseline="hanging">
						Ability:
					</text>
					<g transform="translate(0 52)">
						<rect x="16" width="482" height="48" fill={black} />
						<text x="2" y="1" className="stat-label" dominantBaseline="ideographic">
							ATK Bonus
						</text>
						<rect y="3" width="65" height="42" fill="white" strokeWidth="2" stroke={black} />

						<text x="141" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							1/2 LVL
						</text>
						<rect x="141" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="192" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Abil
						</text>
						<rect x="192" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Class
						</text>
						<rect x="243" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Prof
						</text>
						<rect x="294" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Feat
						</text>
						<rect x="345" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Enh
						</text>
						<rect x="396" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Misc
						</text>
						<rect x="447" y="6" width="47" height="36" fill="white" strokeWidth="0" />
					</g>
					<g transform="translate(0 118)">
						<rect x="16" width="482" height="48" fill={black} />
						<text x="2" y="1" className="stat-label" dominantBaseline="ideographic">
							Damage
						</text>
						<rect y="3" width="165" height="42" fill="white" strokeWidth="2" stroke={black} />

						<text x="243" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Abil
						</text>
						<rect x="243" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="294" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Feat
						</text>
						<rect x="294" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="345" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Enh
						</text>
						<rect x="345" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="396" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Misc
						</text>
						<rect x="396" y="6" width="47" height="36" fill="white" strokeWidth="0" />
						<text x="447" dx="23" y="1" className="stat-label" textAnchor="middle" dominantBaseline="ideographic">
							Misc
						</text>
						<rect x="447" y="6" width="47" height="36" fill="white" strokeWidth="0" />
					</g>
				</g>
				<use transform="translate(0 170)" href="#attack-workspace-ability" />
				<use transform="translate(0 340)" href="#attack-workspace-ability" />
			</g>

			<g id="basic-attacks" transform="translate(1014 762)">
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Basic Attacks
					</text>
				</g>
				<g transform="translate(0 36)" id="attack-workspace-ability">
					<text className="stat-label" y="1" x="31" textAnchor="middle" dominantBaseline="hanging">
						Attack
					</text>
					<text className="stat-label" y="1" x="128" textAnchor="middle" dominantBaseline="hanging">
						Defense
					</text>
					<text className="stat-label" y="1" x="269" textAnchor="middle" dominantBaseline="hanging">
						Weapon or Power
					</text>
					<text className="stat-label" y="1" x="440" textAnchor="middle" dominantBaseline="hanging">
						Damage
					</text>
				</g>
				<g transform="translate(0 50)" id="basic-attack">
					<rect y="0" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
					<rect y="0" x="97" width="62" height="42" fill="white" strokeWidth="2" stroke={black} />
					<text className="label" y="32" x="79" textAnchor="middle" dominantBaseline="ideographic">
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
				<g>
					<rect width="498" height="34" fill={black} />
					<text className="section-title" y="8.5" x="249" fill="white" textAnchor="middle" dominantBaseline="hanging">
						Languages Known
					</text>
				</g>
				<g transform="translate(0 34)" id="attack-workspace-ability">
					<line x1="0" x2="498" y1="36" y2="36" strokeWidth="2" stroke={black} />
				</g>
				<g transform="translate(0 70)" id="attack-workspace-ability">
					<line x1="0" x2="498" y1="36" y2="36" strokeWidth="2" stroke={black} />
				</g>
			</g>
		</svg>
	)
);
