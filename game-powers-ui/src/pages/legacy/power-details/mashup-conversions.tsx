import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';

const defenses: Record<string, string> = {
	AC: 'ac',
	Reflex: 'refl',
	Fortitude: 'fort',
	Willpower: 'will',
};
const abilities = Object.entries({
	Strength: 'STR',
	Constitution: 'CON',
	Dexterity: 'DEX',
	Intelligence: 'INT',
	Wisdom: 'WIS',
	Charisma: 'CHA',
});

function mashupUsage(powerUsage: string) {
	return powerUsage === 'At-Will'
		? 'at-will'
		: powerUsage === 'Encounter'
		? 'encounter'
		: powerUsage === 'Daily'
		? 'daily'
		: powerUsage === 'Item'
		? 'item'
		: 'other';
}

export function foundryMashup(power: LegacyPowerDetails) {
	// const attackType = power.rules.find((rule) => rule.label === 'Attack Type')?.text ?? '';
	const actionType = power.actionType.split(' ')[0].toLowerCase();
	const result = {
		name: power.name,
		type: 'power',
		img: 'icons/svg/item-bag.svg',
		data: {
			sourceId: power.wizardsId,
			type: power.display,
			flavorText: power.flavorText ?? '',
			usage: mashupUsage(power.powerUsage),

			keywords: power.keywords.map((k) => k.toLowerCase()),
			actionType: actionType === 'no' ? 'none' : actionType,
			special: power.rules.find((rule) => rule.label === 'Special')?.text,
			displayOverride: '', // TODO: upgrade to React 18 and render the power display
			trigger: power.rules.find((rule) => rule.label === 'Trigger')?.text,
			prerequisite: power.rules.find((rule) => rule.label === 'Prerequisite')?.text,
			requirement: power.rules.find((rule) => rule.label === 'Requirement')?.text,
			isBasic: !!power.rules.find((rule) => rule.label === '_BasicAttack')?.text,

			effects: buildEffects(power).filter(Boolean),

			usedPools: [],
			grantedPools: [],
			grantedBonuses: [],
			dynamicList: [],
		},
		// flags: {
		// 	legacySource: power,
		// },
	};
	return JSON.stringify(result, undefined, 2);
}

function buildEffects(power: LegacyPowerDetails) {
	const hit = power.rules.find((rule) => rule.label === 'Hit')?.text;
	const miss = power.rules.find((rule) => rule.label === 'Miss')?.text;
	const effect = power.rules.find((rule) => rule.label === 'Effect')?.text;
	const attackRoll = parseAttack(power.rules.find((rule) => rule.label === 'Attack')?.text);
	const result = {
		name: '',
		note: '',
		noteLabel: '',
		target: power.rules.find((rule) => rule.label === 'Target')?.text ?? '',
		typeAndRange: { type: 'melee', size: 'weapon' },
		attackRoll,
		hit: {
			text: (attackRoll ? hit : effect) ?? '',
			healing: null,
			damage: null,
		},
		miss: miss ? { text: miss, healing: null, damage: null } : null,
	};

	if (result.target || result.hit.text || result.miss || result.attackRoll) return [result];
	return [];

	function parseAttack(attack?: string) {
		if (!attack) return null;
		const parts = attack.split(' vs. ');
		if (parts.length !== 2) return null;
		const defense = defenses[parts[1]];
		if (!defense) return null;
		return {
			attack: abilities.reduce((prev, [full, abbrev]) => prev.replace(full, abbrev), parts[0]),
			defense,
		};
	}
}
