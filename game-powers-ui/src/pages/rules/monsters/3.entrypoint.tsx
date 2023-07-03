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
					name="Residuum Spiker"
					levelType="Level 6 Soldier"
					sizeType="Medium magical beast (crystaline)"
					XP={250}
					Initiative="+6"
					Senses="Perception +4; darkvision"
					HP={69}
					Bloodied={34}
					AC={22}
					Fortitude={20}
					Reflex={18}
					Will={16}
					Resist="8 acid"
					Speed="6"
				/>
				<MonsterBlock.Feature type="MeleeBasic" name="Razor Slash" notes="(standard; at-will)">
					+13 vs AC; 2d4+5 damage (crit 4d4+13), and the target is marked until the end of the residuum spiker&rsquo;s
					turn. If a target marked by the residuum spiker makes an attack that does not include the residuum spiker,
					takes 2 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Close" name="Razor Storm" notes="(standard; encounter)">
					Close burst 1; +11 vs Reflex; 1d6+5 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Close" name="Natural Spikes" notes="(minor, at-will)">
					Close blast 1; the target takes 2 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Trailing Languages="None" Str={+8} Dex={+6} Wis={+4} Con={+4} Int={+3} Cha={+2} />
			</MonsterBlock>

			<MonsterBlock>
				<MonsterBlock.Heading
					name="Residuum Darter"
					levelType="Level 6 Skirmisher"
					sizeType="Medium magical beast (crystaline)"
					XP={250}
					Initiative="+7"
					Senses="Perception +3; darkvision"
					HP={71}
					Bloodied={35}
					AC={20}
					Fortitude={19}
					Reflex={18}
					Will={16}
					Speed="6"
				/>
				<MonsterBlock.Feature type="MeleeBasic" name="Razor Slash" notes="(standard; at-will)">
					+11 vs AC; 2d6+7 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature type="Melee" name="Shifting Strike" notes="(at-will)">
					+11 vs. AC; 2d6+7 damage. The darter can shift 1 square before or after the attack.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature name="Ravenous" notes="(minor, at-will, the darter must be bloodied)">
					The darter shifts up to 2 squares closer to a bloodied enemy.
				</MonsterBlock.Feature>
				<MonsterBlock.Trailing Languages="None" Str={+8} Dex={+6} Wis={+4} Con={+4} Int={+3} Cha={+2} />
			</MonsterBlock>

			<MonsterBlock>
				<MonsterBlock.Heading
					name="Residuum Minion"
					levelType="Level 6 Minion"
					sizeType="Medium magical beast (crystaline)"
					XP={63}
					Initiative={+4}
					Senses="Perception +4; darkvision"
					HP={1}
					AC={22}
					Fortitude={18}
					Reflex={17}
					Will={17}
					Speed="6, teleport 3"
				/>
				<MonsterBlock.Feature type="MeleeBasic" name="Razor Slash" notes="(standard; at-will)">
					+11 vs. AC; 5 damage.
				</MonsterBlock.Feature>
				<MonsterBlock.Feature name="Crystaline Wall">
					The residuum minion gains a +2 bonus to its defenses when adjacent to at least one other residuum creature.
				</MonsterBlock.Feature>
				<MonsterBlock.Trailing Languages="None" Str={+5} Dex={+4} Wis={+4} Con={+5} Int={+3} Cha={+4} />
			</MonsterBlock>
		</Monsters>
	);
}

export default createEntry(MyMonsters);
