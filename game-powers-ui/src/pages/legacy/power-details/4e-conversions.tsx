import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';

function foundryUsage(powerUsage: string) {
	return powerUsage === 'At-Will'
		? 'atwill'
		: powerUsage === 'Encounter'
		? 'encounter'
		: powerUsage === 'Daily'
		? 'daily'
		: powerUsage === 'Item'
		? 'item'
		: 'atwill';
}

export function foundry4e(details: LegacyPowerDetails) {
	const attackType = details.rules.find((rule) => rule.label === 'Attack Type')?.text ?? '';
	const actionType = details.actionType.split(' ')[0].toLowerCase();
	const result = {
		name: details.name,
		type: 'power',
		img: attackType.includes('Melee')
			? 'icons/skills/melee/hand-grip-sword-white-brown.webp'
			: attackType.includes('Ranged')
			? 'icons/skills/ranged/arrow-flying-white-blue.webp'
			: attackType.includes('burst')
			? 'icons/magic/fire/barrier-wall-flame-ring-blue.webp'
			: attackType.includes('blast')
			? 'icons/magic/fire/projectile-meteor-salvo-light-pink.webp'
			: attackType.includes('wall')
			? 'icons/magic/fire/flame-burning-fence.webp'
			: 'icons/magic/movement/trail-streak-pink.webp',
		data: {
			description: {
				value: `<p>${details.flavorText}</p>`,
				displayOverride: foundryHtml(details),
			},
			source: details.wizardsId,
			attack: {
				isAttack: false,
			},
			powerSource: '', // TODO
			subName: details.display,
			useType: foundryUsage(details.powerUsage),
			actionType: actionType === 'no' ? 'none' : actionType,
			hit: {
				isDamage: false,
			},
			effectType: {
				channelDiv: details.rules.some((rule) => rule.label === 'Channel Divinity'),
			},
		},
	};
	return JSON.stringify(result);
}

const knownRules = [
	'Attack',
	'Attack Type',
	'Target',
	'Trigger',
	'Requirement',
	'Prerequisite',
	'_ParentFeature',
	'_ChildPower',
	'_DisplayPowers',
	'_ParentPower',
	'_BasicAttack',
];

function foundryHtmlWithHeader(power: LegacyPowerDetails): string {
	return `
<p style="color: #fff" class="ability-usage--${foundryUsage(power.powerUsage)}">${power.name}</p>
${foundryHtml(power, false)}
`;
}

function foundryHtml(power: LegacyPowerDetails, includeId = true): string {
	let isAlt = true;
	const tab = '&nbsp;&nbsp;&nbsp;&nbsp;';

	const attackType = power.rules.find((rule) => rule.label === 'Attack Type')?.text ?? '';
	const target = power.rules.find((rule) => rule.label === 'Target')?.text;
	const attack = power.rules.find((rule) => rule.label === 'Attack')?.text;
	const trigger = power.rules.find((rule) => rule.label === 'Trigger')?.text;
	const requirement = power.rules.find((rule) => rule.label === 'Requirement')?.text;
	const prerequisite = power.rules.find((rule) => rule.label === 'Prerequisite')?.text;

	return `
<p><strong>${power.powerUsage}</strong>${
		power.keywords.length ? ` ✦ <strong>${power.keywords.join(', ')}</strong>` : ''
	}</p>
<p><strong>${power.actionType}</strong>${attackType ? ` ✦ ${attackType}` : ''}</p>
${trigger ? rulesTextLine('Trigger', trigger) : ''}
${target ? rulesTextLine('Target', target) : ''}
${attack ? rulesTextLine('Attack', attack) : ''}
${prerequisite ? rulesTextLine('Prerequisite', prerequisite) : ''}
${requirement ? rulesTextLine('Requirement', requirement) : ''}
${power.rules
	.filter(({ label }) => !knownRules.includes(label))
	.map(({ label, text }) => rulesTextLine(label || '', text))
	.join('\n')}
${power.childPower ? `<div style="padding-left: 8px;">${foundryHtmlWithHeader(power.childPower)}</div>` : ''}
${
	includeId
		? alt(`<p>See <a href="https://4e.dekrey.net/legacy/rule/${power.wizardsId}">${power.wizardsId}</a></p>`)
		: ''
}`;

	function rulesTextLine(label: string, text: string) {
		return alt(
			(label ? `<p><strong>${label}:</strong> ${text}</p>` : `<p>${text}</p>`)
				.replaceAll('\r', '<br/>')
				.replaceAll('\t', tab)
		);
	}
	function alt(text: string) {
		try {
			return isAlt ? `<div class="alt">${text}</div>` : `<div>${text}</div>`;
		} finally {
			isAlt = !isAlt;
		}
	}
}
