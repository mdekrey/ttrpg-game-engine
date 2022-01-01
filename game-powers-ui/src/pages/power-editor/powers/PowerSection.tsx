import { ClassProfile } from 'api/models/ClassProfile';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { PowerEdit } from './PowerEdit';

export function PowerSection({
	classProfile,
	header,
	powers,
	classId,
	onRequestReload,
}: {
	classProfile: ClassProfile;
	header: string;
	powers: PowerTextProfile[];
	classId: string;
	onRequestReload: () => void;
}) {
	return (
		<section>
			{powers.map((p, i) => {
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={p.id}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerEdit
							classProfile={classProfile}
							power={p}
							param={{ classId, powerId: p.id }}
							onRequestReload={onRequestReload}
						/>
					</article>
				);
			})}
		</section>
	);
}
