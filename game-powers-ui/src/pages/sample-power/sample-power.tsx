import { PowerTextBlock, PowerTextBlockProps } from 'components/power';

export function SamplePower({ data: { power } }: { data: { power: PowerTextBlockProps } }) {
	return <PowerTextBlock {...power} />;
}
