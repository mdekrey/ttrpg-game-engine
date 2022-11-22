import { useMemo } from 'react';
import { load as fromYaml } from 'js-yaml';
import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';

export function Power({ children }: { children?: string }) {
	const result = useMemo(() => {
		try {
			return fromYaml(children || '') as Partial<PowerTextBlockProps>;
		} catch (ex) {
			// eslint-disable-next-line no-console
			console.warn(ex);
			return false;
		}
	}, [children]);

	if (!result) {
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
			{...result}
		/>
	);
}
