import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { PowerEdit } from './PowerEdit';

export function PowerSection({
	header,
	powers,
	classId,
}: {
	header: string;
	powers: (PowerTextProfile & { index: number })[];
	classId: string;
}) {
	return (
		<section>
			{powers.map((p, i) => {
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={i}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerEdit power={p} param={{ id: classId, _index: p.index }} />
					</article>
				);
			})}
		</section>
	);
}
