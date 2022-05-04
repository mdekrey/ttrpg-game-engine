import { LegacyRuleText } from 'api/models/LegacyRuleText';
import { inlineObject } from './full-reference-mdx';

type LabelOverride = { label: string; key: string };

export function DisplayRule({ name, description }: { name: string; description?: string | null }) {
	if (!description) return null;
	const lines = description?.split('\r\t');
	return (
		<>
			<p>
				<span className="font-bold">{name}:</span> {lines[0]}
			</p>
			{lines.slice(1).map((line, lineIndex) => (
				<p className="indent-4" key={lineIndex}>
					{line}
				</p>
			))}
		</>
	);
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
		.map((trait) => `<DisplayRule name={${inlineObject(trait.label)}} description={${inlineObject(trait.text)}} />`)
		.join('\n');
}
