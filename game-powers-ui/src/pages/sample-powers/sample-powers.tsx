/* eslint-disable react/no-array-index-key */
import classNames from 'classnames';
import { PowerTextBlock, PowerTextBlockProps } from 'components/power';

export function SamplePowers({
	data: {
		atWill1,
		encounter1,
		daily1,
		encounter3,
		daily5,
		encounter7,
		daily9,
		encounter11,
		encounter13,
		daily15,
		encounter17,
		daily19,
		daily20,
		encounter23,
		daily25,
		encounter27,
		daily29,
	},
}: {
	data: {
		atWill1: PowerTextBlockProps[];
		encounter1: PowerTextBlockProps[];
		daily1: PowerTextBlockProps[];
		encounter3: PowerTextBlockProps[];
		daily5: PowerTextBlockProps[];
		encounter7: PowerTextBlockProps[];
		daily9: PowerTextBlockProps[];
		encounter11: PowerTextBlockProps[];
		encounter13: PowerTextBlockProps[];
		daily15: PowerTextBlockProps[];
		encounter17: PowerTextBlockProps[];
		daily19: PowerTextBlockProps[];
		daily20: PowerTextBlockProps[];
		encounter23: PowerTextBlockProps[];
		daily25: PowerTextBlockProps[];
		encounter27: PowerTextBlockProps[];
		daily29: PowerTextBlockProps[];
	};
}) {
	function Repeat({ header, powers }: { header: string; powers: PowerTextBlockProps[] }) {
		return (
			<>
				{powers.map((p, i) => (
					<div className="mb-4" style={{ pageBreakInside: 'avoid' }} key={`${i}`}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerTextBlock {...p} />
					</div>
				))}
			</>
		);
	}

	return (
		<div className="storybook-md-theme">
			<Repeat header="Level 1 At-Will Powers" powers={atWill1} />
			<Repeat header="Level 1 Encounter Powers" powers={encounter1} />
			<Repeat header="Level 1 Daily Powers" powers={daily1} />
			<Repeat header="Level 3 Encounter Powers" powers={encounter3} />
			<Repeat header="Level 5 Daily Powers" powers={daily5} />
			<Repeat header="Level 7 Encounter Powers" powers={encounter7} />
			<Repeat header="Level 9 Daily Powers" powers={daily9} />
			<Repeat header="Level 11 Encounter Powers" powers={encounter11} />
			<Repeat header="Level 13 Encounter Powers" powers={encounter13} />
			<Repeat header="Level 15 Daily Powers" powers={daily15} />
			<Repeat header="Level 17 Encounter Powers" powers={encounter17} />
			<Repeat header="Level 19 Daily Powers" powers={daily19} />
			<Repeat header="Level 20 Daily Powers" powers={daily20} />
			<Repeat header="Level 23 Encounter Powers" powers={encounter23} />
			<Repeat header="Level 25 Daily Powers" powers={daily25} />
			<Repeat header="Level 27 Encounter Powers" powers={encounter27} />
			<Repeat header="Level 29 Daily Powers" powers={daily29} />
		</div>
	);
}
