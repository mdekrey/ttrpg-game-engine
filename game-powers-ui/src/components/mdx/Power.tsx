import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';

export function Power({ children }: { children?: Partial<PowerTextBlockProps> }) {
	if (!children) {
		return <div className="bg-red-dark p-2 text-white my-4">Invalid Power YAML</div>;
	}
	return (
		<PowerTextBlock
			className="my-4"
			name="Not Given"
			typeInfo="Class Power"
			powerUsage="At-Will"
			keywords={[]}
			rulesText={[]}
			{...children}
		/>
	);
}
