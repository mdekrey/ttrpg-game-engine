import { LegacyRuleText } from 'api/models/LegacyRuleText';
import { wizardsTextToMarkdown } from './wizards-text-to-markdown';

type LabelOverride = { label: string; key: string };

export function ruleMarkdown(name: string, description?: string | null) {
	if (!description) return '';
	return `
<div>

**${name}:** ${wizardsTextToMarkdown(description, { depth: 3 })}

</div>
`;
}

export function RuleListDisplay({
	rules,
	labels,
	className,
}: {
	rules: LegacyRuleText[];
	labels: (string | LabelOverride)[];
	className?: string;
}) {
	return (
		<>
			{labels
				.map((trait) => {
					if (typeof trait === 'string') return rules.find((r) => r.label === trait);
					const found = rules.find((r) => r.label === trait.key);

					return found && { text: found.text, label: trait.label };
				})
				.filter((trait): trait is LegacyRuleText => !!trait?.text)
				.map((trait) => (
					<p key={trait.label} className={className}>
						<span className="font-bold">{trait.label}:</span> {trait.text}
					</p>
				))}
		</>
	);
}

export function ruleListMarkdown(rules: LegacyRuleText[], labels: (string | LabelOverride)[]): string {
	return labels
		.map((trait) => {
			if (typeof trait === 'string') return rules.find((r) => r.label === trait);
			const found = rules.find((r) => r.label === trait.key);

			return found && { text: found.text, label: trait.label };
		})
		.filter((trait): trait is LegacyRuleText => !!trait?.text)
		.map((trait) => ruleMarkdown(trait.label, trait.text))
		.join('\n');
}
