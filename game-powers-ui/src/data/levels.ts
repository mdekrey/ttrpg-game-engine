export type LevelInfo = {
	totalXp: number;
	level: number;
	abilities: null | 1 | 2 | 'all';
	features: string[];
	levelMod: number;
	featsKnown: number;
	atWillPowers: number;
	encounterPowers: number;
	dailyPowers: number;
	utilityPowers: number;
	retrainPower: null | 'encounter' | 'daily';
	skillPoints: number;
	skillMax: number;
};

const levelMod = (level: number) => Math.floor(level / 2);
const featCount = (level: number) => 1 + Math.floor(level / 2);
const startingFeatures: readonly number[] = [1];
const paragonPathFeature: readonly number[] = [11, 16];
const epicDestinyFeature: readonly number[] = [21, 24, 30];
const plusOneAbility: readonly number[] = [4, 8, 14, 18, 24, 28];
const plusAllAbilities: readonly number[] = [21];

function toLevel(
	level: number,
	{
		totalXp,
		powers: totalPowersKnown,
	}: {
		totalXp: number;
		powers: `2/${1 | 2 | 3 | 4}${'*' | ''}/${1 | 2 | 3 | 4}${'*' | ''}/${0 | 1 | 2 | 3 | 4 | 5 | 6}`;
	}
): LevelInfo {
	const features = [
		...(startingFeatures.includes(level) ? ['class features', 'racial traits', 'train starting skills'] : []),
		...(paragonPathFeature.includes(level) ? ['paragon path feature'] : []),
		...(epicDestinyFeature.includes(level) ? ['epic destiny feature'] : []),
	];
	const match =
		/^(?<atWill>[0-9]+)\/(?<encounter>[0-9]+)(?<retrainEncounter>\*|)\/(?<daily>[0-9]+)(?<retrainDaily>\*|)\/(?<utility>[0-9]+)$/.exec(
			totalPowersKnown
		);
	if (!match || !match.groups) throw Error('invalid powers format');

	// If half-level is not added to all skill checks...
	const skillMax = levelMod(level) + 5;
	const skillPoints = skillMax * 5 + level;

	// If half-level is added to all skill checks...
	// const skillMax = 5;
	// const skillPoints = skillMax * 5 + levelMod(level);

	return {
		totalXp,
		level,
		abilities: level === 1 ? 2 : plusAllAbilities.includes(level) ? 'all' : plusOneAbility.includes(level) ? 1 : null,
		features,
		levelMod: levelMod(level),
		featsKnown: featCount(level),
		atWillPowers: parseInt(match.groups.atWill, 10),
		encounterPowers: parseInt(match.groups.encounter, 10),
		dailyPowers: parseInt(match.groups.daily, 10),
		utilityPowers: parseInt(match.groups.utility, 10),
		retrainPower: match.groups.retrainEncounter ? 'encounter' : match.groups.retrainDaily ? 'daily' : null,
		skillMax,
		skillPoints,
	};
}

export const levels: LevelInfo[] = [
	toLevel(1, { totalXp: 0, powers: '2/1/1/0' }),
	toLevel(2, { totalXp: 1_000, powers: '2/1/1/1' }),
	toLevel(3, { totalXp: 2_250, powers: '2/2/1/1' }),
	toLevel(4, { totalXp: 3_750, powers: '2/2/1/1' }),
	toLevel(5, { totalXp: 5_500, powers: '2/2/2/1' }),
	toLevel(6, { totalXp: 7_500, powers: '2/2/2/2' }),
	toLevel(7, { totalXp: 10_000, powers: '2/3/2/2' }),
	toLevel(8, { totalXp: 13_000, powers: '2/3/2/2' }),
	toLevel(9, { totalXp: 16_500, powers: '2/3/3/2' }),
	toLevel(10, { totalXp: 20_500, powers: '2/3/3/3' }),
	toLevel(11, { totalXp: 26_000, powers: '2/4/3/3' }),
	toLevel(12, { totalXp: 32_000, powers: '2/4/3/4' }),
	toLevel(13, { totalXp: 39_000, powers: '2/4*/3/4' }),
	toLevel(14, { totalXp: 47_000, powers: '2/4/3/4' }),
	toLevel(15, { totalXp: 57_000, powers: '2/4/3*/4' }),
	toLevel(16, { totalXp: 69_000, powers: '2/4/3/5' }),
	toLevel(17, { totalXp: 83_000, powers: '2/4*/3/5' }),
	toLevel(18, { totalXp: 99_000, powers: '2/4/3/5' }),
	toLevel(19, { totalXp: 119_000, powers: '2/4/3*/5' }),
	toLevel(20, { totalXp: 143_000, powers: '2/4/3/5' }),
	toLevel(21, { totalXp: 175_000, powers: '2/4/4/5' }),
	toLevel(22, { totalXp: 210_000, powers: '2/4/4/5' }),
	toLevel(23, { totalXp: 255_000, powers: '2/4*/4/5' }),
	toLevel(24, { totalXp: 310_000, powers: '2/4/4/5' }),
	toLevel(25, { totalXp: 375_000, powers: '2/4/4*/5' }),
	toLevel(26, { totalXp: 450_000, powers: '2/4/4/6' }),
	toLevel(27, { totalXp: 550_000, powers: '2/4*/4/6' }),
	toLevel(28, { totalXp: 675_000, powers: '2/4/4/6' }),
	toLevel(29, { totalXp: 825_000, powers: '2/4/4*/6' }),
	toLevel(30, { totalXp: 1_000_000, powers: '2/4/4/6' }),
];
