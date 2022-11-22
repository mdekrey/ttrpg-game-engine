import { LegacyPowerDetails } from 'src/api/models/LegacyPowerDetails';
import { inlineObject } from 'src/components/mdx/FullReferenceMdx';

export function powerMarkdown(power: LegacyPowerDetails | string) {
	if (typeof power === 'string') return `<PowerDetailsSelector id={${power}} />`;
	return `<PowerDetailsSelector id={${inlineObject(power.wizardsId)}} details={${inlineObject(power)}} />`;
}
