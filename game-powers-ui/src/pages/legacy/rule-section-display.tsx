import { LegacyRuleText } from 'api/models/LegacyRuleText';
import { WizardsMarkdown } from './wizards-markdown';
import { wizardsTextToMarkdown } from './wizards-text-to-markdown';

export function RuleSectionDisplay({ rule, title }: { rule: LegacyRuleText | undefined; title?: string | undefined }) {
	if (!rule?.text) return null;
	const displayTitle = title ?? rule.label;
	return (
		<>
			{displayTitle ? (
				<h2 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">{displayTitle}</h2>
			) : null}
			<WizardsMarkdown text={rule.text} depth={3} />
		</>
	);
}

export function sectionMarkdown(rule: LegacyRuleText | undefined, title?: string | undefined) {
	if (!rule?.text) return null;
	const displayTitle = title ?? rule.label;
	return `
${displayTitle ? `## ${displayTitle}` : ''}
${wizardsTextToMarkdown(rule.text, { depth: 3 })}

`;
}
