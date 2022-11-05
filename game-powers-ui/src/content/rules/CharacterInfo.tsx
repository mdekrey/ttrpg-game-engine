export type CharacterInfo = {
	name: string;
	level: number;
	class: string;
	paragonPath?: string;
	epicDestiny?: string;
	totalXp: number;

	race: string;
	size: string;
	age?: string;
	height?: string;
	weight?: string;
	alignment?: string;
	pronouns?: string;
	deity?: string;
	adventuringCompany?: string;

	speed: Modifiers;

	abilities: {
		str: number;
		con: number;
		dex: number;
		int: number;
		wis: number;
		cha: number;
	};

	defenses: {
		ac: Modifiers;
		acConditional: string[];
		fort: Modifiers;
		fortConditional: string[];
		refl: Modifiers;
		reflConditional: string[];
		will: Modifiers;
		willConditional: string[];
	};

	maxHp: number;
	surgesPerDay: number;

	savingThrowModifiers: string[];
	resistances: string[];

	raceFeatures: string[];
	classFeatures: string[];
	feats: string[];
	weaponArmorProficiencies: string[];
	languages: string[];

	skills: Skill[];
};

export type Modifiers = Record<string, number>;
export type ConditionalModifier = {
	amount: number;
	type: string;
	condition: string;
};
export type Skill = {
	name: string;
	modifiers: Modifiers;
};
