import { groupBy } from 'lodash/fp';
import { of } from 'rxjs';
import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { initial, isLoaded, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { StructuredResponses } from 'api/operations/getLegacyClass';
import { ReaderLayout } from 'components/reader-layout';
import { Inset } from 'components/reader-layout/inset';
import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { LegacyRuleText } from 'api/models/LegacyRuleText';
import { Fragment, useMemo } from 'react';
import { LegacyClassDetails } from 'api/models/LegacyClassDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { getArticle } from '../get-article';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';
import { RuleSectionDisplay } from '../rule-section-display';
import { WizardsMarkdown } from '../wizards-markdown';
import { RuleListDisplay } from '../rule-list-display';

type ReasonCode = 'NotFound';

const classTraitSections = [
	['Role', 'Power Source', 'Key Abilities'],
	['Armor Proficiencies', 'Weapon Proficiencies', 'Implements', 'Bonus to Defense'],
	['Hit Points at 1st Level', 'Hit Points per Level Gained', 'Healing Surges'],
	['Trained Skills', 'Class Skills'],
	['Build Options', { key: '_PARSED_CLASS_FEATURE', label: 'Class Features' }],
];

function isOther(rule: LegacyRuleText) {
	if (classTraitSections.flatMap((t) => t).includes(rule.label)) return false;
	if (rule.label.startsWith('_')) return false;
	if (rule.label === 'Creating') return false;
	if (rule.label === 'Class Features') return false;
	if (rule.label === 'Supplemental') return false;
	if (rule.label === 'Short Description') return false;
	if (rule.label === 'Power Name') return false;
	if (rule.label === 'Powers') return false;
	return true;
}

const powerList = [
	'At-Will 1',
	'Encounter 1',
	'Daily 1',
	'Utility 2',
	'Encounter 3',
	'Daily 5',
	'Utility 6',
	'Encounter 7',
	'Daily 9',
	'Utility 10',
	'Encounter 13',
	'Daily 15',
	'Utility 16',
	'Encounter 17',
	'Daily 19',
	'Utility 22',
	'Encounter 23',
	'Daily 25',
	'Encounter 27',
	'Daily 29',
];

export function ClassDetails({
	data: { id: classId, details: preloadedClassDetails },
}: {
	data: { id: string; details?: LegacyClassDetails };
}) {
	const api = useApi();
	const classId$ = useMemoizeObservable([classId, preloadedClassDetails] as const);
	const data = useObservable(
		() =>
			classId$.pipe(
				map(([id, classDetails]) =>
					classDetails
						? of(makeLoaded(classDetails))
						: api
								.getLegacyClass({ params: { id } })
								.pipe(
									map((response) =>
										response.statusCode === 404 ? makeError<ReasonCode>('NotFound' as const) : makeLoaded(response.data)
									)
								)
				),
				switchAll()
			),
		initial as Loadable<StructuredResponses[200]['application/json'], ReasonCode>
	);

	const powers = useMemo(() => {
		if (!isLoaded(data)) return {};

		return groupBy(
			(power) => `${power.powerType === 'Utility' ? 'Utility' : power.powerUsage} ${power.level}`,
			data.value.powers
		);
	}, [data]);

	const powerName = useMemo(() => {
		if (!isLoaded(data)) return 'Power';

		return data.value.details.rules.find((r) => r.label === 'Power Name')?.text ?? 'Power';
	}, [data]);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={({ details, classFeatures, builds }) => (
					<>
						<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
							{details.name} <Sources sources={details.sources} />
						</h1>
						<p className="font-flavor font-bold italic">{details.flavorText}</p>
						<Inset>
							<h2 className="font-header font-bold mt-4 first:mt-0 uppercase">{details.name} Traits</h2>
							{classTraitSections.map((section, sectionIndex) => (
								<section className="mb-2" key={sectionIndex}>
									<RuleListDisplay labels={section} rules={details.rules} />
								</section>
							))}
						</Inset>
						<DynamicMarkdown contents={wizardsTextToMarkdown(details.description, { depth: 1 })} />
						<WizardsMarkdown text={details.description} depth={1} />
						<RuleSectionDisplay
							rule={details.rules.find((r) => r.label === 'Creating')}
							title={`Creating ${getArticle(details.name)} ${details.name}`}
						/>
						{builds.map((build, buildIndex) => (
							<Fragment key={buildIndex}>
								<h3 className="font-header font-bold mt-4 first:mt-0 text-theme text-xl">
									{build.name} <Sources sources={build.sources} />
								</h3>
								<WizardsMarkdown text={build.description} depth={3} />
								<WizardsMarkdown text={build.rules.find((r) => r.label === 'Suggested')?.text} depth={3} />
								<p className="theme-4e-indent">
									<span>Key Abilities:</span> {build.rules.find((r) => r.label === 'Key Abilities')?.text}
								</p>
							</Fragment>
						))}
						<RuleSectionDisplay rule={details.rules.find((r) => r.label === 'Class Features')} />
						{classFeatures.map(({ details: classFeature, powers: featurePowers, subFeatures }, index) => (
							<Fragment key={index}>
								<h3 className="font-header font-bold mt-4 first:mt-0 text-theme text-xl">
									{classFeature.name} <Sources sources={classFeature.sources} />
								</h3>
								<WizardsMarkdown text={classFeature.description} depth={3} />
								{featurePowers.map((power, powerIndex) => (
									<DisplayPower details={power} key={powerIndex} />
								))}
								{subFeatures.map(({ details: subFeatureDetails, powers: subfeaturePowers }, subfeatureIndex) => (
									<Fragment key={subfeatureIndex}>
										<h4 className="font-header font-bold mt-4 first:mt-0 text-black text-lg">
											{subFeatureDetails.name} <Sources sources={subFeatureDetails.sources} />
										</h4>
										<WizardsMarkdown text={subFeatureDetails.description} depth={4} />
										{subfeaturePowers.map((power, powerIndex) => (
											<DisplayPower details={power} key={powerIndex} />
										))}
									</Fragment>
								))}
							</Fragment>
						))}
						<WizardsMarkdown text={details.rules.find((r) => r.label === 'Supplemental')?.text} depth={1} />
						{details.rules
							.filter(isOther)
							.filter((rule) => !!rule.text)
							.map((rule, index) => (
								<RuleSectionDisplay rule={rule} key={index} />
							))}
						<RuleSectionDisplay rule={details.rules.find((r) => r.label === 'Powers')} />
						{powerList
							.filter((category) => !!powers[category] && powers[category].length > 0)
							.map((category) => (
								<Fragment key={category}>
									<div style={{ breakInside: 'avoid' }}>
										{/* This div around the first power and the header helps the page layout in Chrome */}
										<h3
											className="font-header font-bold mt-4 first:mt-0 text-theme text-2xl"
											style={{ breakAfter: 'avoid' }}>
											{details.name} {category} {powerName}
										</h3>
										<DisplayPower details={powers[category][0]} />
									</div>
									{powers[category].slice(1).map((power, powerIndex) => (
										<DisplayPower details={power} key={powerIndex} />
									))}
								</Fragment>
							))}
					</>
				)}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}
