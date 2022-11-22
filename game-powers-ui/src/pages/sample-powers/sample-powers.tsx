/* eslint-disable react/no-array-index-key */
import classNames from 'classnames';
import groupBy from 'lodash/fp/groupBy';
import zip from 'lodash/fp/zip';
import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';
import { ReaderLayout } from 'src/components/reader-layout';

type PowerProfileProps = {
	level: number;
	usage: string;
	[other: string]: unknown;
};

function PowerSection({ header, powers }: { header: string; powers: PowerTextBlockProps[] }) {
	return (
		<section>
			{powers.map((p, i) => (
				<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={`${i}`}>
					{i === 0 && (
						<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
					)}
					<PowerTextBlock {...p} />
				</article>
			))}
		</section>
	);
}

export function SamplePowers({
	data: {
		powerText,
		powerProfile: { powers },
	},
}: {
	data: {
		powerText: PowerTextBlockProps[];
		powerProfile: { powers: PowerProfileProps[] };
	};
}) {
	const zipped = zip(powerText, powers);
	const groups = groupBy(([, profile]) => `Level ${profile!.level} ${profile!.usage} Powers`, zipped);

	return (
		<ReaderLayout>
			{Object.keys(groups).map((header) => (
				<PowerSection header={header} key={header} powers={groups[header].map(([power]) => power!)} />
			))}
		</ReaderLayout>
	);
}
