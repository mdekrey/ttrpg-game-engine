import { LegacyRuleText } from 'src/api/models/LegacyRuleText';
import { mdxComponents } from 'src/components/layout/mdx-components';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';
import { wizardsTextToMarkdown } from './wizards-text-to-markdown';

type LabelOverride = { label: string; key: string };

export function Rule({ name, description }: { name: string; description?: string | null }) {
	if (!description) return null;
	const Strong = mdxComponents.strong;
	return (
		<div>
			<Strong>{name}</Strong>: {description /* TODO - this used to be markdown, does it still need to be? */}
		</div>
	);
}

export function RuleList({ rules, labels }: { rules: LegacyRuleText[]; labels: (string | LabelOverride)[] }) {
	return (
		<>
			{labels
				.map((trait) => {
					if (typeof trait === 'string') return rules.find((r) => r.label === trait);
					const found = rules.find((r) => r.label === trait.key);

					return found && { text: found.text, label: trait.label };
				})
				.filter((trait): trait is LegacyRuleText => !!trait?.text)
				.map((trait, index) => (
					<Rule name={trait.label} description={trait.text} key={index} />
				))}
		</>
	);
}
