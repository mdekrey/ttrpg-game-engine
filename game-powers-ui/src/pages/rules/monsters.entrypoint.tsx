import chunk from 'lodash/fp/chunk';
import { Fragment } from 'react';
import { MonsterBlock } from 'src/components/monster/monster-block';
import { createEntry } from 'src/lib/markdown-entry';
import { recurse } from 'src/core/jsx/recurse';
import { JsxMutator, pipeJsx } from 'src/core/jsx/pipeJsx';

const mergeWidth: JsxMutator = (previous) => <td width="50%">{previous}</td>;
const rowPairs = (mutator: JsxMutator): JsxMutator => {
	return (elem) => {
		const c = mutator(elem);
		if (c.type === Fragment && c.props.children) {
			const entries = chunk(2, c.props.children);
			return (
				<>
					{entries.map((children, idx) => (
						<tr key={idx} className="break-inside-avoid">
							{children}
						</tr>
					))}
				</>
			);
		}
		return c;
	};
};

function Monsters() {
	return (
		<>
			<div className="pt-16 print:pt-0" />
			<table className="border-spacing-[0.5in] -m-[0.5in] border-separate" style={{ columnSpan: 'all' }}>
				<tbody className="align-top">
					{pipeJsx(
						<>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Gray Wolf"
									levelType="Level 2 Skirmisher"
									sizeType="Medium natural beast"
									XP={125}
									Initiative="+5"
									Senses="Perception +7; low-light vision"
									HP={38}
									Bloodied={19}
									AC={16}
									Fortitude={14}
									Reflex={14}
									Will={13}
									Speed={8}
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+7 vs. AC; 1d6 + 2 damage, or 2d6 + 2 damage against a prone target.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Combat Advantage">
									If the gray wolf has combat advantage against the target, the target is also knocked prone on a hit.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+2} Dex={+3} Wis={+2} Con={+3} Int={-3} Cha={+1} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Dire Wolf"
									levelType="Level 5 Skirmisher"
									sizeType="Large natural beast (mount)"
									XP={200}
									Initiative="+7"
									Senses="Perception +9; low-light vision"
									HP={67}
									Bloodied={33}
									AC={19}
									Fortitude={18}
									Reflex={17}
									Will={16}
									Speed={8}
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+10 vs. AC; 1d8 + 4 damage, or 2d8 + 4 damage against a prone target.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Combat Advantage">
									The dire wolf gains combat advantage against a target that has one or more of the dire wolf’s allies
									adjacent to it. If the dire wolf has combat advantage against the target, the target is also knocked
									prone on a hit.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									name="Pack Hunter"
									notes="(while mounted by a friendly rider of 5th level or higher; at-will)"
									keywords="Mount">
									The dire wolf’s rider gains combat advantage against an enemy if it has at least one ally other than
									its mount adjacent to the target.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+6} Dex={+5} Wis={+4} Con={+6} Int={-1} Cha={+2} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kruthik Hatchling"
									levelType="Level 2 Minion"
									sizeType="Small natural beast (reptile)"
									XP={31}
									Initiative="+3"
									Senses="Perception +0; low-light vision, tremorsense 10"
									Extra={{
										'Gnashing Horde': `aura 1; an enemy that ends its turn in the aura takes 2 damage.`,
									}}
									HP={1}
									AC={15}
									Fortitude={13}
									Reflex={15}
									Will={12}
									Speed="8, burrow 2 (tunneling), climb 8"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Claw" notes="(standard; at-will)">
									+5 vs. AC; 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+1} Dex={+3} Wis={+0} Con={+1} Int={-3} Cha={-2} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kruthik Young"
									levelType="Level 2 Brute"
									sizeType="Small natural beast (reptile)"
									XP={125}
									Initiative={+4}
									Senses="Perception +1; low-light vision, tremorsense 10"
									Extra={{ 'Gnashing Horde': 'aura 1; an enemy that ends its turn in the aura takes 2 damage.' }}
									HP={43}
									Bloodied={21}
									AC={15}
									Fortitude={13}
									Reflex={14}
									Will={11}
									Speed="8, burrow 2, climb 8"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Claw" notes="(standard; at-will)">
									+5 vs. AC; 1d8 + 2 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+3} Dex={+4} Wis={+1} Con={+2} Int={-2} Cha={-1} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kruthik Adult"
									levelType="Level 4 Brute"
									sizeType="Medium natural beast (reptile)"
									XP={175}
									Initiative={+6}
									Senses="Perception +4; low-light vision, tremorsense 10"
									Extra={{ 'Gnashing Horde': 'aura 1; an enemy that ends its turn in the aura takes 2 damage.' }}
									HP={67}
									Bloodied={33}
									AC={17}
									Fortitude={14}
									Reflex={15}
									Will={13}
									Speed="6, burrow 3 (tunneling), climb 6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Claw" notes="(standard; at-will)">
									+8 vs. AC; 1d10 + 3 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Ranged"
									name="Toxic Spikes"
									notes="(standard; recharge ⚄ ⚅)"
									keywords="Poison">
									The kruthik makes 2 attacks against two different targets: ranged 5; +7 vs. AC; 1d8 + 4 damage, and
									the target takes ongoing 5 poison damage and is slowed (save ends both).
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+5} Dex={+6} Wis={+4} Con={+5} Int={-1} Cha={+1} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kruthik Hive Lord"
									levelType="Level 6 Elite Controller (Leader)"
									sizeType="Large natural beast (reptile)"
									XP={500}
									Initiative={+7}
									Senses="Perception +4; low-light vision, tremorsense 10"
									Extra={{
										'Hive Frenzy': 'aura 2; allied kruthiks in the aura deal double damage with basic attacks.',
									}}
									HP={148}
									Bloodied={74}
									AC={22}
									Fortitude={21}
									Reflex={20}
									Will={17}
									Speed="6, burrow 3 (tunneling), climb 6"
									ActionPoints={1}
								/>
								<MonsterBlock.Feature name="Claw" notes="(standard; at-will)">
									+11 vs. AC; 1d10 + 5 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Acid Blast" notes="(standard; at-will)" keywords="Acid">
									Close blast 5; targets enemies; +9 vs. Fortitude; 1d6 + 4 acid damage, and the target takes ongoing 5
									acid damage and is weakened (save ends both).
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+8} Dex={+7} Wis={+4} Con={+7} Int={+0} Cha={+3} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Guardian of the Temple"
									levelType="Level 4 Solo Soldier"
									sizeType="Large natural beast"
									XP={875}
									Initiative={+6}
									Senses="Perception +3"
									HP={224}
									Bloodied={112}
									AC={20}
									Fortitude={20}
									Reflex={17}
									Will={16}
									Speed="6"
									ActionPoints={1}
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Claw" notes="(standard; at-will)">
									+9 vs. AC; 1d8 + 8 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Double Claw" notes="(standard; at-will)">
									Two attacks: +9 vs. AC; 1d8 + 8 damage. If both attacks hit the same target, the target is grabbed.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Crushing Squeeze" notes="(standard; at-will)">
									One grabbed creature: +7 vs. Fortitude; 4d8 + 18 damage. Miss: half damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+7} Dex={+4} Wis={+3} Con={+5} Int={-3} Cha={-1} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Minion"
									levelType="Level 1 Minion"
									sizeType="Small natural humanoid"
									XP={25}
									Initiative={+3}
									Senses="Perception +1; darkvision"
									HP={1}
									AC={15}
									Fortitude={11}
									Reflex={13}
									Will={11}
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Javelin" notes="(standard; at-will)" keywords="Weapon">
									+5 vs. AC; 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="RangedBasic" name="Javelin" notes="(standard; at-will)" keywords="Weapon">
									Ranged 10/20; +5 vs. AC; 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Stealth +4, Thievery +4"
									Str={-1}
									Dex={+3}
									Wis={+1}
									Con={+1}
									Int={-1}
									Cha={+0}
									Equipment="hide armor, light shield, 3 javelin"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Skirmisher"
									levelType="Level 1 Skirmisher"
									sizeType="Small natural humanoid"
									XP={100}
									Initiative={+5}
									Senses="Perception +0; darkvision"
									HP={27}
									Bloodied={13}
									AC={15}
									Fortitude={11}
									Reflex={14}
									Will={13}
									ExtraDefense="see also trap sense"
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Spear" notes="(standard; at-will)" keywords="Weapon">
									+6 vs. AC; 1d8 damage; see also mob attack.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Combat Advantage">
									The kobold skirmisher deals an extra 1d6 damage on melee and ranged attacks against any target it has
									combat advantage against.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Mob Attack">
									The kobold skirmisher gains a +1 bonus to attack rolls per kobold ally adjacent to the target.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Acrobatics +7, Stealth +9, Thievery +9"
									Str={-1}
									Dex={+3}
									Wis={+0}
									Con={+0}
									Int={-2}
									Cha={+2}
									Equipment="hide armor, spear"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Slinger"
									levelType="Level 1 Artillery"
									sizeType="Small natural humanoid"
									XP={100}
									Initiative={+3}
									Senses="Perception +1; darkvision"
									HP={24}
									Bloodied={12}
									AC={13}
									Fortitude={12}
									Reflex={14}
									Will={12}
									ExtraDefense="see also trap sense"
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Dagger" notes="(standard; at-will)" keywords="Weapon">
									+5 vs. AC; 1d4 + 3 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="RangedBasic" name="Sling" notes="(standard; at-will)" keywords="Weapon">
									Ranged 10/20; +6 vs. AC; 1d6 + 3 damage; see also special shot.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Special Shot">
									The kobold slinger can fire special ammunition from its sling. It typically carries 3 rounds of
									special shot, chosen from the types listed below. A special shot attack that hits deals normal damage
									and has an additional effect depending on its type:
									<br />
									<span className="font-bold">Stinkpot:</span> The target takes a –2 penalty to attack rolls (save
									ends).
									<br />
									<span className="font-bold">Firepot (Fire):</span> The target takes ongoing 2 fi re damage (save
									ends).
									<br />
									<span className="font-bold">Gluepot:</span> The target is immobilized (save ends).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Acrobatics +8, Stealth +10, Thievery +10"
									Str={-1}
									Dex={+3}
									Wis={+1}
									Con={+1}
									Int={-1}
									Cha={+0}
									Equipment="leather armor, dagger, sling with 20 bullets and 3 rounds of special shot (see above)"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Dragonshield"
									levelType="Level 2 Soldier"
									sizeType="Small natural humanoid"
									XP={125}
									Initiative={+4}
									Senses="Perception +2; darkvision"
									HP={36}
									Bloodied={18}
									AC={18}
									Fortitude={14}
									Reflex={13}
									Will={13}
									ExtraDefense="see also trap sense"
									Speed="6"
								/>
								<MonsterBlock.Feature
									type="MeleeBasic"
									name="Short Sword"
									notes="(standard; at-will)"
									keywords="Weapon">
									+7 vs. AC; 1d6 + 3 damage, and the target is marked until the end of the kobold dragonshield’s next
									turn.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									name="Dragonshield Tactics"
									notes="(immediate reaction, when an adjacent
								enemy shifts away or an enemy moves adjacent; at-will)">
									The kobold dragonshield shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Mob Attack">
									The kobold skirmisher gains a +1 bonus to attack rolls per kobold ally adjacent to the target.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Acrobatics +5, Stealth +7, Thievery +7"
									Str={+3}
									Dex={+2}
									Wis={+2}
									Con={+2}
									Int={+0}
									Cha={+1}
									Equipment="scale armor, heavy shield, short sword"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Wyrmpriest"
									levelType="Level 3 Artillery (Leader)"
									sizeType="Small natural humanoid"
									XP={150}
									Initiative={+4}
									Senses="Perception +4; darkvision"
									HP={36}
									Bloodied={18}
									AC={17}
									Fortitude={13}
									Reflex={15}
									Will={15}
									ExtraDefense="see also trap sense"
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Spear" notes="(standard; at-will)" keywords="Weapon">
									+7 vs. AC; 1d8 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Ranged" name="Energy Orb" notes="(standard; at-will)" keywords="see text">
									Ranged 10; +6 vs. Reflex; 1d10 + 3 damage of a chosen type (based on the dragon served).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Close" name="Incite Faith" notes="(minor; encounter)">
									Close burst 10; kobold allies in the burst gain 5 temporary hit points and shift 1 square.{' '}
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Close" name="Dragon Breath" notes="(standard; encounter)">
									Close blast 3; +6 vs. Fortitude; 1d10 + 3 damage of a chosen type (based on the dragon served). Miss:
									Half damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Stealth +10, Thievery +10"
									Str={+0}
									Dex={+4}
									Wis={+4}
									Con={+2}
									Int={+0}
									Cha={+2}
									Equipment="hide armor, spear, bone mask"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Kobold Slyblade"
									levelType="Level 4 Lurker"
									sizeType="Small natural humanoid"
									XP={175}
									Initiative={+10}
									Senses="Perception +3; darkvision"
									HP={42}
									Bloodied={21}
									AC={18}
									Fortitude={12}
									Reflex={16}
									Will={14}
									ExtraDefense="see also trap sense"
									Speed="6"
								/>
								<MonsterBlock.Feature
									type="MeleeBasic"
									name="Short Sword"
									notes="(standard; at-will)"
									keywords="Weapon">
									+9 vs. AC; 1d6 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Twin Slash" notes="(standard; at-will)" keywords="Weapon">
									Requires combat advantage; the kobold slyblade makes 2 short sword attacks. If both attacks hit the
									same target, the target takes ongoing 5 damage (save ends).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Combat Advantage">
									The kobold skirmisher deals an extra 1d6 damage on melee and ranged attacks against any target it has
									combat advantage against.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									name="Sly Dodge"
									notes="(immediate interrupt, when targeted by a melee or a ranged attack; at-will)">
									The kobold slyblade redirects the attack to an adjacent kobold minion.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Shifty" notes="(minor; at-will)">
									The kobold shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Trap Sense">
									The kobold gains a +2 bonus to all defenses against traps.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Languages="Common, Draconic"
									Skills="Acrobatics +11, Stealth +13, Thievery +13"
									Str={+1}
									Dex={+6}
									Wis={+3}
									Con={+3}
									Int={+1}
									Cha={+4}
									Equipment="leather armor, 2 short swords"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Human Bandit"
									levelType="Level 2 Skirmisher"
									sizeType="Medium natural humanoid"
									XP={125}
									Initiative={+6}
									Senses="Perception +1"
									HP={37}
									Bloodied={18}
									AC={16}
									Fortitude={12}
									Reflex={14}
									Will={12}
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Mace" notes="(standard; at-will)" keywords="Weapon">
									+4 vs. AC; 1d8 + 1 damage, and the human bandit shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="RangedBasic" name="Dagger" notes="(standard; at-will)" keywords="Weapon">
									Ranged 5/10; +6 vs. AC; 1d4 + 3 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Dazing Strike" notes="(standard; encounter)" keywords="Weapon">
									Requires mace; +4 vs. AC; 1d8 + 1 damage, the target is dazed until the end of the human bandit’s next
									turn, and the human bandit shifts 1 square.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Combat Advantage">
									The human bandit deals an extra 1d6 damage on melee and ranged attacks against any target it has
									combat advantage against.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Str={+2}
									Dex={+4}
									Wis={+1}
									Con={+2}
									Int={+1}
									Cha={+2}
									Equipment="leather armor, mace, 4 daggers"
								/>
							</MonsterBlock>

							<MonsterBlock>
								<MonsterBlock.Heading
									name="Human Rabble"
									levelType="Level 2 Minion"
									sizeType="Medium natural humanoid"
									XP={31}
									Initiative={+0}
									Senses="Perception +0"
									HP={1}
									AC={15}
									Fortitude={13}
									Reflex={11}
									Will={11}
									ExtraDefense="see also mob rule"
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Club" notes="(standard; at-will)" keywords="Weapon">
									+6 vs. AC; 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Mob Rule">
									The human rabble gains a +2 power bonus to all defenses while at least two other human rabble are
									within 5 squares of it.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+2} Dex={+0} Wis={+0} Con={+1} Int={-1} Cha={+0} Equipment="club" />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Human Lackey"
									levelType="Level 7 Minion"
									sizeType="Medium natural humanoid"
									XP={75}
									Initiative={+3}
									Senses="Perception +4"
									HP={1}
									AC={19}
									Fortitude={17}
									Reflex={14}
									Will={15}
									ExtraDefense="see also mob rule"
									Speed="6"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Club" notes="(standard; at-will)" keywords="Weapon">
									+12 vs. AC; 6 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Mob Rule">
									The human lackey gains a +2 power bonus to all defenses while at least two other human rabble are
									within 5 squares of it.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Str={+6}
									Dex={+3}
									Wis={+4}
									Con={+5}
									Int={+3}
									Cha={+4}
									Equipment="leather armor, club"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Human Guard"
									levelType="Level 3 Soldier"
									sizeType="Medium natural humanoid"
									XP={150}
									Initiative={+5}
									Senses="Perception +6"
									HP={47}
									Bloodied={23}
									AC={18}
									Fortitude={16}
									Reflex={15}
									Will={14}
									Speed="5"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Halberd" notes="(standard; at-will)" keywords="Weapon">
									Reach 2; +10 vs. AC; 1d10 + 3 damage, and the target is marked until the end of the human guard’s next
									turn.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Ranged" name="Crossbow" notes="(standard; at-will)" keywords="Weapon">
									Ranged 15/30; +9 vs. AC; 1d8 + 2 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Powerful Strike"
									notes="(standard; recharge ⚄ ⚅)"
									keywords="Weapon">
									Requires halberd; reach 2; +10 vs. AC; 1d10 + 7 damage, and the target is knocked prone.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Skills="Streetwise +7"
									Str={+4}
									Dex={+3}
									Wis={+1}
									Con={+3}
									Int={+1}
									Cha={+2}
									Equipment="chainmail, halberd, crossbow with 20 bolts"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Human Berserker"
									levelType="Level 4 Brute"
									sizeType="Medium natural humanoid"
									XP={175}
									Initiative={+3}
									Senses="Perception +2"
									HP={66}
									Bloodied={33}
									HealthExtra="see also battle fury"
									AC={15}
									Fortitude={15}
									Reflex={14}
									Will={14}
									Speed="7"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Greataxe" notes="(standard; at-will)" keywords="Weapon">
									+7 vs. AC; 1d12 + 4 damage (crit 1d12 + 16).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Ranged" name="Handaxe" notes="(standard; at-will)" keywords="Weapon">
									Ranged 5/10; +5 vs. AC; 1d6 + 3 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Battle Fury" notes="(free, when first bloodied; encounter)">
									The human berserker makes a melee basic attack with a +4 bonus to the attack roll and deals an extra
									1d6 damage on a hit.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Skills="Athletics +9, Endurance +9"
									Str={+5}
									Dex={+3}
									Wis={+2}
									Con={+5}
									Int={+2}
									Cha={+3}
									Equipment="hide armor, greataxe, 2 handaxes"
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Pseudodragon"
									levelType="Level 3 Lurker"
									sizeType="Tiny natural beast (reptile)"
									XP={150}
									Initiative={+9}
									Senses="Perception +8"
									HP={40}
									Bloodied={20}
									AC={17}
									Fortitude={14}
									Reflex={15}
									Will={14}
									Speed="4, fl y 8 (hover); see also flyby attack"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+8 vs. AC; 1d8 + 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="MeleeBasic"
									name="Sting"
									notes="(standard; recharge ⚃ ⚄ ⚅)"
									keywords="Poison">
									+8 vs. AC; 1d8 + 4 damage, and ongoing 5 poison damage (save ends).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Flyby Attack" notes="(standard; at-will)">
									The pseudodragon fl ies up to 8 squares and makes one melee basic attack at any point during that
									movement. The pseudodragon doesn’t provoke opportunity attacks when moving away from the target of the
									attack.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Invisibility"
									notes="(standard; recharges when the pseudodragon is damaged)"
									keywords="Illusion">
									As long as the pseudodragon doesn’t move, it is invisible.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing
									Skills="Insight +8, Stealth +10"
									Str={+2}
									Dex={+5}
									Wis={+3}
									Con={+4}
									Int={-1}
									Cha={+4}
								/>
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Spitting Drake"
									levelType="Level 3 Artillery"
									sizeType="Medium natural beast (reptile)"
									XP={150}
									Initiative={+5}
									Senses="Perception +3"
									HP={38}
									Bloodied={19}
									AC={17}
									Fortitude={14}
									Reflex={16}
									Will={14}
									Resist="10 acid"
									Speed="7"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+6 vs. AC; 1d6 + 2 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Ranged" name="Caustic Spit" notes="(standard; at-will)" keywords="Acid">
									Ranged 10; +8 vs. Refl ex; 1d10 + 4 acid damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+3} Dex={+5} Wis={+3} Con={+3} Int={-3} Cha={+2} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Rage Drake"
									levelType="Level 5 Brute"
									sizeType="Large natural beast (mount, reptile)"
									XP={200}
									Initiative={+3}
									Senses="Perception +3"
									HP={77}
									Bloodied={38}
									HealthExtra="see also bloodied rage"
									AC={17}
									Fortitude={17}
									Reflex={15}
									Will={15}
									Immune="fear (while bloodied only)"
									Speed="8"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+9 vs. AC; 1d10 + 4 damage; see also bloodied rage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Claw" notes="(standard; at-will)">
									+8 vs. AC; 1d6 + 4 damage; see also bloodied rage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Raking Charge" notes="(standard; at-will)">
									When the rage drake charges, it makes two claw attacks against a single target.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Bloodied Rage" notes="(while bloodied)">
									The rage drake gains a +2 bonus to attack rolls and deals an extra 5 damage per attack.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									name="Raging Mount"
									notes="(while bloodied and mounted by a friendly rider of 5th level or higher; at-will)"
									keywords="Mount">
									The rage drake grants its rider a +2 bonus to attack rolls and damage rolls with melee attacks.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+6} Dex={+3} Wis={+3} Con={+5} Int={-2} Cha={+3} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Giant Ant: Hive Queen"
									levelType="Level 5 Elite Controller (Leader)"
									sizeType="Large natural beast"
									XP={400}
									Initiative={+6}
									Senses="Perception +2; low-light vision, tremorsense 10"
									HP={132}
									Bloodied={66}
									AC={19}
									Fortitude={19}
									Reflex={16}
									Will={17}
									Immune="fear"
									Speed="6, climb 2"
									SavingThrows="+2"
									ActionPoints={1}
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+10 vs. AC; 1d10 + 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="MeleeBasic" name="Kick" notes="(minor; at-will)">
									Reach 2; +9 vs. Reflex; 3 damage, and the hive queen pushes the target 3 squares.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Hive Queen Frenzy"
									notes="(free, when any giant ant within 10 squares of the hive queen drops to 0 hit points; at-will)">
									The queen shifts 2 squares and uses kick.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Close"
									name="Acidic Cloud"
									notes="(standard; encounter)"
									keywords="Acid, Zone">
									Close burst 4; the burst creates a zone of caustic gas that lasts until the end of the encounter. Any
									enemy that starts its turn within the zone takes 2 acid damage for each giant ant within the zone. The
									zone is centered on the hive queen and moves with her.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Area"
									name="Acidic Blast"
									notes="(standard; recharge ⚃ ⚄ ⚅)"
									keywords="Acid">
									Area burst 3 within 10; targets enemies; +7 vs. Will; 1d6 + 2 acid damage, and the target is dazed
									(save ends). Miss: Half damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature name="Call to Arms" notes="(when first bloodied; encounter)">
									Four new giant ant hive workers appear and act to defend their queen. Each ant appears within 5
									squares of the hive queen, and acts on the queen’s initiative count.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+3} Dex={+3} Wis={+2} Con={+6} Int={-2} Cha={+4} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Giant Ant: Hive Worker"
									levelType="Level 1 Minion Skirmisher"
									sizeType="Medium natural beast"
									XP={25}
									Initiative={+0}
									Senses="Perception -1; low-light vision, tremorsense 10"
									HP={1}
									AC={15}
									Fortitude={13}
									Reflex={13}
									Will={10}
									Speed="6, climb 6, burrow 2 (tunneling)"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)">
									+6 vs. AC; 4 damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Hive Worker Frenzy"
									notes="(free, when any giant ant within 10 squares of the hive worker drops to 0 hit points; at-will)">
									The hive worker shifts 2 squares.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+3} Dex={+2} Wis={-1} Con={+2} Int={-4} Cha={-3} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Giant Ant: Hive Warrior"
									levelType="Level 2 Skirmisher"
									sizeType="Medium natural beast"
									XP={125}
									Initiative={+6}
									Senses="Perception -1; low-light vision, tremorsense 10"
									HP={36}
									Bloodied={18}
									AC={16}
									Fortitude={14}
									Reflex={15}
									Will={11}
									Speed="Speed 8, climb 8"
								/>
								<MonsterBlock.Feature
									type="MeleeBasic"
									name="Piercing Bite"
									notes="(standard; at-will)"
									keywords="Acid">
									+7 vs. AC; 1d8 + 4 damage. The hive warrior’s attack deals 1d10 extra acid damage to any target that
									already has ongoing acid damage.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Hive Warrior Frenzy"
									notes="(free, when any giant ant within 10 squares of the hive warrior drops to 0 hit points; at-will)">
									The warrior is no longer marked or cursed, and it shifts 2 squares.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+3} Dex={+4} Wis={+0} Con={+2} Int={-3} Cha={-2} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Giant Ant: Hive Soldier"
									levelType="Level 2 Skirmisher"
									sizeType="Medium natural beast"
									XP={125}
									Initiative={+6}
									Senses="Perception +0; low-light vision, tremorsense 10"
									HP={46}
									Bloodied={23}
									HealthExtra="see also death convulsion"
									AC={18}
									Fortitude={16}
									Reflex={15}
									Will={12}
									Speed="Speed 6, climb 6"
								/>
								<MonsterBlock.Feature
									type="MeleeBasic"
									name="Grasping Mandibles"
									notes="(standard; usable only while the hive  soldier does not have a creature grabbed; at-will)">
									+10 vs. AC; 1d8 + 3 damage, and the target is grabbed.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Acid Sting" notes="(standard; at-will)">
									Targets a creature grabbed by the hive soldier; +10 vs. AC; 1d6 + 3 acid damage, and ongoing 5 acid
									damage (save ends).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Close"
									name="Death Convulsion"
									notes="(when the hive soldier drops to 0 hit points)">
									Close burst 1; targets enemies; +8 vs. Reflex; the target is knocked prone.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Hive Soldier Frenzy"
									notes="(free, when any giant ant within 10 squares of the hive soldier drops to 0 hit points; at-will)">
									The soldier gains a +2 bonus to attack rolls until the end of its next turn.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+4} Dex={+3} Wis={+0} Con={+3} Int={-3} Cha={-2} />
							</MonsterBlock>
							<MonsterBlock>
								<MonsterBlock.Heading
									name="Giant Ant: Winged Drone"
									levelType="Level 4 Skirmisher"
									sizeType="Medium natural beast"
									XP={175}
									Initiative={+6}
									Senses="Perception +2; low-light vision, tremorsense 10"
									HP={55}
									Bloodied={27}
									AC={18}
									Fortitude={15}
									Reflex={17}
									Will={12}
									Speed="Speed 8, climb 8, fly 8"
								/>
								<MonsterBlock.Feature type="MeleeBasic" name="Acid Sting" notes="(standard; at-will)">
									+9 vs. AC; 1d6 + 2 acid damage, and ongoing 5 acid damage (save ends).
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Melee"
									name="Hive Drone Frenzy"
									notes="(free, when any giant ant within 10 squares of the winged drone drops to 0 hit points; at-will)">
									The drone shifts 2 squares and uses acid sting.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature type="Melee" name="Flyby Attack" notes="(standard; at-will)">
									The winged drone flies 8 squares and makes one acid sting at any point during that movement. The drone
									doesn’t provoke opportunity attacks when moving away from the target of the attack.
								</MonsterBlock.Feature>
								<MonsterBlock.Feature
									type="Close"
									name="Shredding Wings"
									notes="(standard; usable only while bloodied; encounter)">
									Close blast 2; +8 vs. AC; 3d6 + 4 damage, and the winged drone loses its fly speed until the end of
									the encounter.
								</MonsterBlock.Feature>
								<MonsterBlock.Trailing Str={+4} Dex={+6} Wis={+2} Con={+4} Int={-2} Cha={-1} />
							</MonsterBlock>
						</>,
						rowPairs(recurse(mergeWidth))
					)}
				</tbody>
			</table>
		</>
	);
}

export default createEntry(Monsters);
