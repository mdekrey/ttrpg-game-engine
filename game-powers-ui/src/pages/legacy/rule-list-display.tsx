import { LegacyRuleText } from 'api/models/LegacyRuleText';

export function RuleListDisplay({
	rules,
	labels,
	className,
}: {
	rules: LegacyRuleText[];
	labels: string[];
	className?: string;
}) {
	return (
		<>
			{labels
				.map((trait) => rules.find((r) => r.label === trait))
				.filter((trait): trait is LegacyRuleText => !!trait?.text)
				.map((trait) => (
					<p key={trait.label} className={className}>
						<span className="font-bold">{trait.label}:</span> {trait.text ?? <span className="italic">Unknown</span>}
					</p>
				))}
		</>
	);
}
