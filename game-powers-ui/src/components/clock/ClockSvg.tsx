import { useMemo } from 'react';
import { arc, pie, PieArcDatum } from 'd3-shape';

export function ClockSvg({
	className,
	currentTicks,
	totalTicks,
}: {
	className?: string;
	currentTicks: number;
	totalTicks: number;
}) {
	const radius = 48;
	const padding = 2;
	const clockArc = useMemo(() => arc<void, PieArcDatum<unknown>>().innerRadius(0).outerRadius(radius), [radius]);
	const clockPie = useMemo(() => {
		const clockPieData = Array(totalTicks).fill(1);
		return pie<void, number>()(clockPieData);
	}, [totalTicks]);

	return (
		<svg className={className} viewBox="0 0 100 100">
			<title>
				{currentTicks} of {totalTicks}
			</title>
			<g transform={`translate(${padding / 2 + radius} ${padding / 2 + radius})`}>
				{clockPie.map((piece) => (
					<path
						key={piece.index}
						d={clockArc(piece)!}
						fill="currentcolor"
						stroke="black"
						strokeWidth={Math.min(padding, totalTicks <= 8 ? 2 : totalTicks <= 20 ? 1 : 0.5)}
						className={piece.index < currentTicks ? 'text-gray-700' : 'text-gray-50'}
					/>
				))}
			</g>
		</svg>
	);
}
