import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { DisplayPower } from './display-power';
import { inlineObject } from '../full-reference-mdx';
import { buildSelector } from '../loader-selector';

export const PowerDetailsSelector = buildSelector('getLegacyPower', DisplayPower);

export function displayPowerMarkdown(power: LegacyPowerDetails) {
	return `<PowerDetailsSelector id={${inlineObject(power.wizardsId)}} details={${inlineObject(power)}} />`;
}
