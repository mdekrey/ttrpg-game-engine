import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { inlineObject } from '../full-reference-mdx';

export function powerMarkdown(power: LegacyPowerDetails | string) {
	if (typeof power === 'string') return `<PowerDetailsSelector id={${power}} />`;
	return `<PowerDetailsSelector id={${inlineObject(power.wizardsId)}} details={${inlineObject(power)}} />`;
}
