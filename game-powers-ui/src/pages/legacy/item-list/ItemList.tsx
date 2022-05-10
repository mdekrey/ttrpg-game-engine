import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { LegacyGearSummary } from 'api/models/LegacyGearSummary';
import { StructuredResponses } from 'api/operations/getLegacyItems';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyRuleSummary } from 'api/models/LegacyRuleSummary';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { Fragment, useMemo } from 'react';
import { groupBy } from 'lodash';
import { LegacyArmorSummary } from 'api/models/LegacyArmorSummary';
import { LegacyWeaponSummary } from 'api/models/LegacyWeaponSummary';
import { wizardsSort } from '../wizards-sort';

export function ItemList() {
	const api = useApi();
	const data = useObservable(
		() => api.getLegacyItems().pipe(map((response) => makeLoaded(response.data))),
		initial as Loadable<StructuredResponses[200]['application/json'], never>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => <LoadedItemList loaded={loaded} />}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}

const integerFormatting = new Intl.NumberFormat('en-US', {});

function LoadedItemList({ loaded }: { loaded: StructuredResponses[200]['application/json'] }) {
	return (
		<>
			<GearList gear={loaded.gear} />
			<ArmorList armor={loaded.armor} />
			<WeaponList weapons={loaded.weapons} />
			<MainHeader>Other Item List</MainHeader>
			<ul className="list-disc ml-6 theme-4e-list">
				{sortBy<LegacyRuleSummary>(wizardsSort, loaded.others).map(({ wizardsId, name, flavorText }) => (
					<li key={wizardsId} className="my-1">
						<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
							{name}
						</a>
						{flavorText ? <>&mdash; {flavorText}</> : null}
					</li>
				))}
			</ul>
		</>
	);
}

function GearList({ gear: gearList }: { gear: LegacyGearSummary[] }) {
	const gear = useMemo(() => groupBy(gearList, (c) => c.category), [gearList]);

	return (
		<>
			{sortBy((c) => c, Object.keys(gear)).map((category) => (
				<Fragment key={category}>
					<MainHeader>{category} List</MainHeader>

					<table className="w-full border-collapse">
						<tbody>
							{sortBy<LegacyGearSummary>(({ name }) => name, gear[category]).map(
								({ wizardsId, name, gold, silver, copper, weight, count }) => (
									<tr
										key={wizardsId}
										className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
										<td>
											<a href={`/legacy/rule/${wizardsId}`}>
												{name}
												{count === 1 ? '' : `, x${count}`}
											</a>
										</td>
										<td className="text-right">
											{weight ? (
												<>
													{integerFormatting.format(weight)} {weight === 1 ? 'lb' : 'lbs'}
												</>
											) : null}
										</td>
										<td className="text-right">
											{gold ? <>{integerFormatting.format(gold)} gp</> : null} {silver ? <>{silver} sp</> : null}{' '}
											{copper ? <>{copper} cp</> : null}
										</td>
									</tr>
								)
							)}
						</tbody>
					</table>
				</Fragment>
			))}
		</>
	);
}

function ArmorList({ armor: armorList }: { armor: LegacyArmorSummary[] }) {
	return (
		<>
			<MainHeader>Armor</MainHeader>
			<table className="w-full border-collapse">
				<tbody>
					{sortBy<LegacyArmorSummary>(
						[
							({ armorCategory }) => (armorCategory === '' ? 'unknown' : ''),
							({ armorType }) => (armorType === 'Shield' ? 'shield' : ''),
							({ minimumEnhancementBonus }) => minimumEnhancementBonus ?? 0,
							({ armorBonus }) => armorBonus,
						],
						armorList
					).map(
						({
							wizardsId,
							name,
							armorCategory,
							armorType,
							armorBonus,
							minimumEnhancementBonus,
							check,
							speed,
							gold,
							weight,
						}) => (
							<tr
								key={wizardsId}
								className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
								<td>
									<a href={`/legacy/rule/${wizardsId}`}>{name}</a>
								</td>
								<td>{armorCategory}</td>
								<td>{armorType}</td>
								<td>{armorBonus}</td>
								<td>{minimumEnhancementBonus}</td>
								<td>{check}</td>
								<td>{speed}</td>
								<td className="text-right">{integerFormatting.format(gold)} gp</td>
								<td className="text-right">
									{weight ? (
										<>
											{integerFormatting.format(weight)} {weight === 1 ? 'lb' : 'lbs'}
										</>
									) : null}
								</td>
							</tr>
						)
					)}
				</tbody>
			</table>
		</>
	);
}

function WeaponList({ weapons: weaponList }: { weapons: LegacyWeaponSummary[] }) {
	const weapons = sortBy<LegacyWeaponSummary>(
		[
			({ weaponCategory }) => weaponCategory,
			({ handsRequired }) => handsRequired,
			({ size }) => (size === 'Medium' ? '' : size),
			({ name }) => name,
		],
		weaponList
	);
	return (
		<>
			<MainHeader>Weapon</MainHeader>
			<table className="w-full border-collapse">
				<tbody>
					{weapons.map(
						(
							{
								wizardsId,
								name,
								weaponCategory,
								handsRequired,
								proficiencyBonus,
								damage,
								range,
								gold,
								weight,
								group,
								properties,
								size,
							},
							index
						) => (
							<Fragment key={wizardsId}>
								{index === 0 ||
								weapons[index - 1].weaponCategory !== weaponCategory ||
								weapons[index - 1].handsRequired !== handsRequired ||
								weapons[index - 1].size !== size ? (
									<>
										<tr>
											<th colSpan={8}>
												{weaponCategory}
												{handsRequired}
											</th>
										</tr>
										<tr>
											<th>Name</th>
											<th>Prof.</th>
											<th>Damage</th>
											<th>Range</th>
											<th>Price</th>
											<th>Weight</th>
											<th>Group</th>
											<th>Properties</th>
										</tr>
									</>
								) : null}
								<tr className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
									<td>
										<a href={`/legacy/rule/${wizardsId}`}>{name}</a>
									</td>
									<td>{typeof proficiencyBonus === 'number' ? `+${proficiencyBonus}` : <>&mdash;</>}</td>
									<td>{damage}</td>
									<td>{range || <>&mdash;</>}</td>
									<td className="text-right">
										{typeof gold === 'number' ? `${integerFormatting.format(gold)} gp` : <>&mdash;</>}
									</td>
									<td className="text-right">
										{typeof weight === 'number' ? (
											`${integerFormatting.format(weight)} ${weight === 1 ? 'lb' : 'lbs'}`
										) : (
											<>&mdash;</>
										)}
									</td>
									<td>{group}</td>
									<td>{properties}</td>
								</tr>
							</Fragment>
						)
					)}
				</tbody>
			</table>
		</>
	);
}
