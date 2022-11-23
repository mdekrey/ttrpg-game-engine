import { groupBy } from 'lodash/fp';
import { Inset } from 'src/components/reader-layout/inset';
import { LegacyRuleText } from 'src/api/models/LegacyRuleText';
import { Fragment, useMemo } from 'react';
import { LegacyClassDetails } from 'src/api/models/LegacyClassDetails';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { FlavorText } from 'src/components/reader-layout/FlavorText';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { getArticle } from '../get-article';
import { Sources } from '../sources';
import { sectionMarkdown } from '../rule-section-display';
import { RuleList } from '../rule-list-display';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { powerMap } from '../power-details/powerMarkdown';
import { mdxComponents } from 'src/components/layout/mdx-components';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';

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

const typeOrder = ['At-Will', 'Encounter', 'Daily', 'Utility'];
const powerList = Array(30)
	.fill(0)
	.map((_, index) => index + 1)
	.flatMap((level) => typeOrder.map((type) => `${type} ${level}`));

export function ClassDetails({ details: fullDetails }: { details: LegacyClassDetails }) {
	const powers = useMemo(() => {
		return groupBy(
			(power) => `${power.powerType === 'Utility' ? 'Utility' : power.powerUsage} ${power.level}`,
			fullDetails.powers
		);
	}, [fullDetails]);

	const powerName = useMemo(() => {
		return fullDetails.details.rules.find((r) => r.label === 'Power Name')?.text ?? 'Power';
	}, [fullDetails]);

	const H2 = mdxComponents.h2;
	const H3 = mdxComponents.h3;

	const { details, builds, classFeatures } = fullDetails;
	return (
		<>
			<MainHeader>
				{details.name} <Sources sources={details.sources} />
			</MainHeader>
			<FlavorText>{details.flavorText}</FlavorText>
			<Inset>
				<H3>{details.name.toUpperCase()} TRAITS</H3>

				{classTraitSections.map((section, index) => (
					<section className="mb-4" key={index}>
						<RuleList rules={details.rules} labels={section} />
					</section>
				))}
			</Inset>
			<ConvertedMarkdown md={wizardsTextToMarkdown(details.description, { depth: 1 })} />
			<ConvertedMarkdown
				md={
					sectionMarkdown(
						details.rules.find((r) => r.label === 'Creating'),
						`Creating ${getArticle(details.name)} ${details.name}`
					) ?? ''
				}
			/>
			{builds.map((build, index) => (
				<Fragment key={index}>
					<H3>
						{build.name} <Sources sources={build.sources} />
					</H3>
					<ConvertedMarkdown md={wizardsTextToMarkdown(build.description, { depth: 4 })} />
					<ConvertedMarkdown
						md={wizardsTextToMarkdown(build.rules.find((r) => r.label === 'Suggested')?.text, { depth: 4 })}
					/>
					Key Abilities: {build.rules.find((r) => r.label === 'Key Abilities')?.text}
				</Fragment>
			))}
			<ConvertedMarkdown
				md={
					sectionMarkdown(
						details.rules.find((r) => r.label === 'Class Features'),
						undefined,
						1
					) ?? ''
				}
			/>
			{classFeatures.map(({ details: classFeature, powers: featurePowers, subFeatures }, index) => (
				<Fragment key={index}>
					<H2>
						{classFeature.name} <Sources sources={classFeature.sources} />
					</H2>

					<ConvertedMarkdown md={wizardsTextToMarkdown(classFeature.description, { depth: 3 })} />

					{featurePowers.map(powerMap)}

					{subFeatures.map(({ details: subFeatureDetails, powers: subfeaturePowers }, index2) => (
						<Fragment key={index2}>
							<H3>
								{subFeatureDetails.name} <Sources sources={subFeatureDetails.sources} />
							</H3>

							<ConvertedMarkdown md={wizardsTextToMarkdown(subFeatureDetails.description, { depth: 4 })} />

							{subfeaturePowers.map(powerMap)}
						</Fragment>
					))}
				</Fragment>
			))}
			<ConvertedMarkdown
				md={`${wizardsTextToMarkdown(details.rules.find((r) => r.label === 'Supplemental')?.text, { depth: 1 })}
${details.rules
	.filter(isOther)
	.filter((rule) => !!rule.text)
	.map((rule) => sectionMarkdown(rule, undefined, 1))
	.join('\n')}
${sectionMarkdown(
	details.rules.find((r) => r.label === 'Powers'),
	undefined,
	1
)}`}
			/>
			{powerList
				.filter((category) => !!powers[category] && powers[category].length > 0)
				.map((category) => (
					<Fragment key={category}>
						<div style={{ breakInside: 'avoid' }}>
							{/* This div around the first power and the header helps the page layout in Chrome */}
							<h3 className="font-header font-bold mt-4 text-theme text-2xl" style={{ breakAfter: 'avoid' }}>
								{details.name} {category} {powerName}
							</h3>
							<PowerDetailsSelector id={powers[category][0].wizardsId} details={powers[category][0]} />
						</div>
						{powers[category].slice(1).map((power, powerIndex) => (
							<PowerDetailsSelector id={power.wizardsId} details={power} key={powerIndex} />
						))}
					</Fragment>
				))}
		</>
	);
}
