import { LegacyRuleText } from 'src/api/models/LegacyRuleText';
import { wizardsTextToMarkdown } from './wizards-text-to-markdown';

export function sectionMarkdown(rule: LegacyRuleText | undefined, title?: string | undefined, depth = 2) {
	if (!rule?.text) return null;
	const displayTitle = title ?? rule.label;

	const header = '#'.repeat(depth);
	return `
${displayTitle ? `${header} ${displayTitle}` : ''}
${wizardsTextToMarkdown(rule.text, { depth: depth + 1 })}

`;
}
