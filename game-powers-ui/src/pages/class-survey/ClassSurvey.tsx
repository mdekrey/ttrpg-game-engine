import { ClassProfile } from 'api/models/ClassProfile';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { Card } from 'components/card/card';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { useApi } from 'core/hooks/useApi';
import type { Dictionary } from 'lodash';
import { groupBy } from 'lodash/fp';
import { useState } from 'react';
import { ClassSurveyForm } from './form/class-survey-form';

function PowerSection({ header, powers }: { header: string; powers: PowerTextProfile[] }) {
	return (
		<section>
			{powers.map((p, i) => {
				const { powerUsage, attackType, ...text } = p.text;
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={i}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerTextBlock
							{...text}
							powerUsage={powerUsage as PowerType}
							attackType={attackType as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
						/>
					</article>
				);
			})}
		</section>
	);
}

export function ClassSurvey() {
	const [powers, setPowers] = useState<null | Dictionary<PowerTextProfile[]>>(null);
	const api = useApi();

	return (
		<div className="p-8 bg-gray-50 min-h-screen">
			<ClassSurveyForm onSubmit={submitClassProfile} />
			{powers && (
				<Card className="mt-4">
					<div className="storybook-md-theme">
						{Object.keys(powers).map((header) => (
							<PowerSection header={header} key={header} powers={powers[header]} />
						))}
					</div>
				</Card>
			)}
		</div>
	);

	async function submitClassProfile(classProfile: ClassProfile) {
		const response = await api.generatePowers({}, classProfile, 'application/json').toPromise();
		if (response.statusCode !== 200) return;
		const groups = groupBy(
			(block) => `Level ${block.profile.level} ${block.profile.usage} Powers`,
			response.data.powerTextBlocks
		);
		setPowers(groups);
	}
}
