import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';

export function SamplePower({ data: { power } }: { data: { power: PowerTextBlockProps } }) {
	return <PowerTextBlock {...power} />;
}
