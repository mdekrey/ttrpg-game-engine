import { LegacyPowerDetails } from 'src/api/models/LegacyPowerDetails';
import { PowerDetailsSelector } from './power.selector';

export function powerMap(power: LegacyPowerDetails | string, index: number) {
	if (typeof power === 'string') return <PowerDetailsSelector id={power} key={index} />;
	return <PowerDetailsSelector key={index} id={power.wizardsId} details={power} />;
}
