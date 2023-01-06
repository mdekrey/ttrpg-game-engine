import { MonsterBlock } from 'src/components/monster/monster-block';
import { createEntry } from 'src/lib/markdown-entry';
import { MagicItemDetails } from 'src/pages/legacy/magic-item-details/MagicItemDetails';
import { PowerDetailsSelector } from 'src/pages/legacy/power-details/power.selector';
import { Monsters } from './shared';

function MyMonsters() {
	return (
		<Monsters>
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
				<MonsterBlock.Heading
					name="Young Green Dragon"
					levelType="Level 5 Solo Skirmisher"
					sizeType="Large natural magical beast (dragon)"
					XP={1000}
					Initiative="+7"
					Senses="Perception +10; darkvision"
					HP={260}
					Bloodied={130}
					HealthExtra="see also bloodied breath"
					AC={21}
					Fortitude={17}
					Reflex={19}
					Will={17}
					Resist="15 poison"
					SavingThrows="+5"
					Speed="8, fly 10 (hover), overland flight 15; see also flyby attack"
					ActionPoints={2}
				/>
				<MonsterBlock.Feature type="MeleeBasic" name="Bite" notes="(standard; at-will)" keywords="Poison">
					Reach 2; +10 vs. AC; 1d8 + 5 damage, and ongoing 5 poison damage (save ends).
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="MeleeBasic" name="Claw" notes="(standard; at-will)">
					Reach 2; +10 vs. AC; 1d6 + 5 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Melee" name="Double Attack" notes="(standard; at-will)">
					The dragon makes two claw attacks.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Melee" name="Flyby Attack" notes="(standard; recharge ⚄ ⚅)">
					The dragon flies up to 10 squares and makes a bite attack at any point during the move without provoking an
					opportunity attack from the target.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature
					type="Melee"
					name="Tail Sweep"
					notes="(immediate reaction, if an adjacent enemy does not move on its turn; at-will)">
					+8 vs. Reflex; 1d8 + 5 damage, and the target is knocked prone.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Ranged" name="Luring Glare" notes="(minor 1/round; at-will)" keywords="Charm, Gaze">
					Range 10; +8 vs. Will; the target slides 2 squares.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Close" name="Breath Weapon" notes="(standard; recharge ⚄ ⚅)" keywords="Poison">
					Close blast 5; +8 vs. Fortitude; 1d10 + 3 poison damage, and the target takes ongoing 5 poison damage and is
					slowed (save ends both). Aftereffect: The target is slowed (save ends).
				</MonsterBlock.Feature>
				<MonsterBlock.Feature
					type="Close"
					name="Bloodied Breath"
					notes="(free, when first bloodied; encounter)"
					keywords="Poison">
					The dragon’s breath weapon recharges, and the dragon uses it immediately.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Close" name="Frightful Presence" notes="(standard; encounter)" keywords="Fear">
					Close burst 5; targets enemies; +8 vs. Will; the target is stunned until the end of the dragon’s next turn.
					Aftereff ect: The target takes a –2 penalty to attack rolls (save ends).
				</MonsterBlock.Feature>
				<MonsterBlock.Trailing
					Languages="Common, Draconic"
					Skills="Bluff +15, Diplomacy +10, Insight +15, Intimidate +10"
					Str={+4}
					Dex={+7}
					Wis={+5}
					Con={+5}
					Int={+4}
					Cha={+5}
				/>
			</MonsterBlock>

			<MonsterBlock>
				<MonsterBlock.Heading
					name="Kobold Dragoncult Minion"
					levelType="Level 4 Minion"
					sizeType="Small natural humanoid"
					XP={44}
					Initiative={+3}
					Senses="Perception +1; darkvision"
					HP={1}
					AC={18}
					Fortitude={14}
					Reflex={16}
					Will={14}
					Speed="6"
				/>
				<MonsterBlock.Feature type="MeleeBasic" name="Javelin" notes="(standard; at-will)" keywords="Weapon">
					+8 vs. AC; 5 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="RangedBasic" name="Javelin" notes="(standard; at-will)" keywords="Weapon">
					Ranged 10/20; +8 vs. AC; 5 damage.
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

			<PowerDetailsSelector
				id={'Custom'}
				details={{
					wizardsId: 'Custom',
					name: `Ss'ren's Instincts`,
					flavorText: `Ss'ren trusts his instincts, managing to not be in the spot everyone thought he was in just in time to avoid a devastating blow.`,
					type: 'Power',
					description: '',
					shortDescription: '',
					display: `Ss'ren Utility 2`,
					powerUsage: 'Encounter',
					actionType: 'Immediate Interrupt',
					powerType: 'Utility',
					encounterUses: 1,
					level: '2',
					sources: [],
					rules: [
						{ label: 'Attack Type', text: 'Personal' },
						{ label: 'Trigger', text: 'You are hit by an attack' },
						{
							label: 'Effect',
							text: 'The target must reroll the attack against you; the result of the new roll must be used. If they miss, you shift a number of squares equal to your INT.',
						},
					],
					keywords: ['Arcane'],
				}}
			/>

			<MagicItemDetails
				details={{
					details: {
						wizardsId: 'ID_FMP_MAGIC_ITEM_3181',
						name: 'Bracers of Archery (heroic tier)',
						flavorText: 'These leather armbands enhance your potency with bows and crossbows.',
						type: 'Magic Item',
						description: '',
						shortDescription: '',
						sources: ["Adventurer's Vault"],
						rules: [
							{ label: 'Gold', text: '1800' },
							{ label: 'Magic Item Type', text: 'Arms Slot Item' },
							{ label: 'Tier', text: '' },
							{ label: '_Item_Set_ID', text: '' },
							{ label: 'Armor', text: '' },
							{ label: 'Weapon', text: '' },
							{ label: 'Item Slot', text: 'Arms' },
							{ label: 'Requirement', text: '' },
							{ label: 'Critical', text: '' },
							{ label: 'Special', text: '' },
							{
								label: 'Power',
								text: 'Power (Daily): Minor Action. Ignore cover on your next attack this turn when using a bow or crossbow.',
							},
							{ label: '_Rarity', text: 'Uncommon' },
							{ label: 'Granted Powers', text: '' },
							{
								label: 'Property',
								text: 'Gain a +2 item bonus to damage rolls when attacking with a bow or crossbow.',
							},
						],
					},
					level: 6,
					powers: [],
				}}
			/>

			<MagicItemDetails
				details={{
					details: {
						wizardsId: 'ID_FMP_MAGIC_ITEM_3357',
						name: 'Wildrunners (heroic tier)',
						flavorText: 'Crafted from the skins of wild plains animals, these boots lend you extraordinary speed.',
						type: 'Magic Item',
						description: '',
						shortDescription: '',
						sources: ["Adventurer's Vault"],
						rules: [
							{ label: 'Gold', text: '840' },
							{ label: 'Magic Item Type', text: 'Feet Slot Item' },
							{ label: 'Tier', text: '' },
							{ label: '_Item_Set_ID', text: '' },
							{ label: 'Armor', text: '' },
							{ label: 'Weapon', text: '' },
							{ label: 'Item Slot', text: 'Feet' },
							{ label: 'Requirement', text: '' },
							{ label: 'Critical', text: '' },
							{ label: 'Special', text: '' },
							{
								label: 'Power',
								text: 'Power (Daily): Free Action. Use this power when you run. Enemies do not gain combat advantage against you.',
							},
							{ label: '_Rarity', text: 'Uncommon' },
							{ label: 'Granted Powers', text: '' },
							{ label: 'Property', text: 'When you run, move your speed + 4 instead of speed + 2.' },
						],
					},
					level: 4,
					powers: [],
				}}
			/>

			<MagicItemDetails
				details={{
					details: {
						wizardsId: 'ID_FMP_MAGIC_ITEM_1000',
						name: 'Gauntlets of Ogre Power (heroic tier)',
						flavorText:
							'These oversized armored gloves increase your strength and can be activated to increase your damage.',
						type: 'Magic Item',
						description: '',
						shortDescription: '',
						sources: ["Player's Handbook"],
						rules: [
							{ label: 'Gold', text: '1000' },
							{ label: 'Magic Item Type', text: 'Hands Slot Item' },
							{ label: 'Tier', text: '' },
							{ label: '_Item_Set_ID', text: '' },
							{ label: 'Armor', text: '' },
							{ label: 'Weapon', text: '' },
							{ label: 'Item Slot', text: 'Hands' },
							{ label: 'Requirement', text: '' },
							{ label: 'Critical', text: '' },
							{ label: 'Special', text: '' },
							{
								label: 'Power',
								text: 'Power (Daily): Free Action. Use this power when you hit with a melee attack. Add a +5 power bonus to the damage roll.',
							},
							{ label: '_Rarity', text: 'Uncommon' },
							{ label: 'Granted Powers', text: '' },
							{
								label: 'Property',
								text: 'Gain a +1 item bonus to Athletics checks and Strength ability checks (but not Strength attacks).',
							},
						],
					},
					level: 5,
					powers: [],
				}}
			/>

			<MagicItemDetails
				details={{
					details: {
						wizardsId: 'ID_FMP_MAGIC_ITEM_1069',
						name: 'Elven Cloak +2',
						flavorText: 'This cloak of swirling leaves, crafted in the elven tradition, increases your stealth.',
						type: 'Magic Item',
						description: '',
						shortDescription: '',
						sources: ["Player's Handbook"],
						rules: [
							{ label: 'Gold', text: '2600' },
							{ label: 'Magic Item Type', text: 'Neck Slot Item' },
							{ label: 'Tier', text: '' },
							{ label: '_Item_Set_ID', text: '' },
							{ label: 'Armor', text: '' },
							{ label: 'Weapon', text: '' },
							{ label: 'Item Slot', text: 'Neck' },
							{ label: 'Requirement', text: '' },
							{ label: 'Critical', text: '' },
							{ label: 'Special', text: '' },
							{ label: 'Power', text: '' },
							{ label: '_Rarity', text: 'Common' },
							{ label: 'Enhancement', text: '+2 Fortitude, Reflex, and Will' },
							{ label: 'Granted Powers', text: '' },
							{
								label: 'Property',
								text: "You gain an item bonus to Stealth checks equal to the cloak's enhancement bonus.",
							},
						],
					},
					level: 7,
					powers: [],
				}}
			/>
		</Monsters>
	);
}

export default createEntry(MyMonsters);
