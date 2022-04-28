import classNames from 'classnames';

export function Sources({ className, sources, asBlock }: { className?: string; sources: string[]; asBlock?: boolean }) {
	return (
		<span
			className={classNames(className, 'italic text-xs print:hidden text-gray-800 font-medium font-text', {
				block: asBlock,
			})}>
			{asBlock ? 'Source(s): ' : null}
			<span>{sources.join(', ')}</span>
		</span>
	);
}
