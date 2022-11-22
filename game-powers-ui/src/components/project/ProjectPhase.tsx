import { ReactNode } from 'react';
import classNames from 'classnames';
import { recurse } from 'src/core/jsx/recurse';
import { pipeJsx } from 'src/core/jsx/pipeJsx';
import { mergeStyles } from 'src/core/jsx/mergeStyles';
import { ClockSvg } from 'src/components/clock/ClockSvg';

export type CheckDifficulty = {
	check: string;
	result: ReactNode;
};

export const ProjectPhase = ({
	currentTicks = 0,
	totalTicks,
	advancement,
	check = null,
	cost = null,
	goal,
	children,
	...props
}: {
	goal: string;
	currentTicks?: number;
	totalTicks: number;
	check?: ReactNode;
	advancement: true | CheckDifficulty[];
	cost?: ReactNode;
} & JSX.IntrinsicElements['section']) => {
	return (
		<section {...props}>
			<header
				className={classNames(
					'bg-blue-dark',
					'font-header text-white',
					'flex justify-between items-baseline px-2 pt-0.5'
				)}>
				<span className="text-lg leading-none py-1 font-bold">{goal}</span>
			</header>

			<div className="bg-gradient-to-r from-tan-fading px-2">
				<ClockSvg className="w-16 h-16 m-2 float-right" currentTicks={currentTicks} totalTicks={totalTicks} />
				{pipeJsx(<>{children}</>, recurse(mergeStyles(<p className="font-info" />)))}
				<dl className="font-info">
					{def('Cost', cost)}
					{def('Total Ticks', totalTicks)}
					{/* {def('Check', check)} */}
					{def(
						'Advancement',
						advancement === true ? (
							'Automatic'
						) : (
							<>
								{check}
								<ul className="ml-4">
									{advancement.map((checkResult, index) => (
										// eslint-disable-next-line react/no-array-index-key
										<li key={index}>
											<span className="font-bold">{checkResult.check}:</span> {checkResult.result}
										</li>
									))}
								</ul>
							</>
						)
					)}
				</dl>
			</div>
			<div className="clear-both" />
		</section>
	);

	function def(label: ReactNode, value: ReactNode) {
		if (value === null || value === '') return null;
		return (
			<>
				<dt className="float-left mr-1 font-bold">{label}:</dt>
				<dd>{value}</dd>
			</>
		);
	}
};
