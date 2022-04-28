import { LegacyRuleText } from 'api/models/LegacyRuleText';

type LabelOverride = { label: string; key: string };

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
						<span className="font-bold">{trait.label}:</span> {trait.text ?? <span className="italic">Unknown</span>}
					</p>
				))}
		</>
	);
}
