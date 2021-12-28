import { PowerTextProfile } from 'api/models/PowerTextProfile';
import classNames from 'classnames';
import { PowerTextBlock } from 'components/power';
import { powerTextBlockToProps } from 'components/power/PowerTextBlock';

export function PowerSection({ header, powers }: { header: string; powers: PowerTextProfile[] }) {
	return (
		<section>
			{powers.map((p, i) => {
				return (
					// eslint-disable-next-line react/no-array-index-key
					<article className="mb-4" style={{ pageBreakInside: 'avoid' }} key={p.id}>
						{i === 0 && (
							<h2 className={classNames('font-header font-bold', 'mt-4 first:mt-0', 'text-theme text-xl')}>{header}</h2>
						)}
						<PowerTextBlock {...powerTextBlockToProps(p.text)} />
					</article>
				);
			})}
		</section>
	);
}
