import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { LegacyGearSummary } from 'api/models/LegacyGearSummary';
import { StructuredResponses } from 'api/operations/getLegacyItems';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { Fragment, useMemo } from 'react';
import { groupBy } from 'lodash';
import { LegacyArmorSummary } from 'api/models/LegacyArmorSummary';
import { LegacyWeaponSummary } from 'api/models/LegacyWeaponSummary';

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
			{/* <MainHeader>Other Item List</MainHeader>
			<ul className="list-disc ml-6 theme-4e-list">
				{sortBy<LegacyRuleSummary>(wizardsSort, loaded.others).map(({ wizardsId, name, flavorText }) => (
					<li key={wizardsId} className="my-1">
						<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
							{name}
						</a>
						{flavorText ? <>&mdash; {flavorText}</> : null}
					</li>
				))}
			</ul> */}
		</>
	);
}

function GearList({ gear: gearList }: { gear: LegacyGearSummary[] }) {
	const gear = useMemo(() => groupBy(gearList, (c) => c.category), [gearList]);

	return (
		<>
			{sortBy((c) => (c === 'Gear' ? '' : c), Object.keys(gear)).map((category) => (
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
	const armorGroups = useMemo(
		() =>
			groupBy(armorList, ({ armorCategory, armorType }) =>
				armorCategory === '' ? 'Barding' : armorType === 'Shield' ? 'Shield' : 'Armor'
			),
		[armorList]
	);
	const armorSort = sortBy<LegacyArmorSummary>([
		({ minimumEnhancementBonus }) => minimumEnhancementBonus ?? 0,
		({ armorBonus }) => armorBonus,
	]);

	return (
		<div className="full-page mt-4">
			<MainHeader>Armor List</MainHeader>
			<table className="w-full border-collapse">
				{armorHeader()}
				<tbody>{armorSort(armorGroups.Armor).map(armorRow)}</tbody>
			</table>

			<MainHeader>Shield List</MainHeader>
			<table className="w-full border-collapse">
				{armorHeader()}
				<tbody>{armorSort(armorGroups.Shield).map(armorRow)}</tbody>
			</table>

			<MainHeader>Barding List</MainHeader>
			<table className="w-full border-collapse">
				{armorHeader()}
				<tbody>{armorSort(armorGroups.Barding).map(armorRow)}</tbody>
			</table>
		</div>
	);

	function armorHeader() {
		return (
			<thead>
				<tr className="bg-theme text-white">
					<th className="px-2 font-bold align-bottom">Name</th>
					<th className="px-2 font-bold align-bottom">Category</th>
					<th className="px-2 font-bold align-bottom">Type</th>
					<th className="px-2 font-bold align-bottom">Armor Bonus</th>
					<th className="px-2 font-bold align-bottom">Minimum Enhancement Bonus</th>
					<th className="px-2 font-bold align-bottom">Check</th>
					<th className="px-2 font-bold align-bottom">Speed</th>
					<th className="px-2 font-bold align-bottom">Price</th>
					<th className="px-2 font-bold align-bottom">Weight</th>
				</tr>
			</thead>
		);
	}

	function armorRow({
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
	}: LegacyArmorSummary) {
		return (
			<tr
				key={wizardsId}
				className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
				<td>
					<a href={`/legacy/rule/${wizardsId}`}>{name}</a>
				</td>
				<td>{armorCategory}</td>
				<td>{armorType}</td>
				<td className="text-center">+{armorBonus}</td>
				<td className="text-center">{minimumEnhancementBonus ? `+${minimumEnhancementBonus}` : ''}</td>
				<td className="text-center">{check}</td>
				<td className="text-center">{speed}</td>
				<td className="text-right">{integerFormatting.format(gold)} gp</td>
				<td className="text-right">
					{weight ? (
						<>
							{integerFormatting.format(weight)} {weight === 1 ? 'lb' : 'lbs'}
						</>
					) : null}
				</td>
			</tr>
		);
	}
}

function WeaponList({ weapons: weaponList }: { weapons: LegacyWeaponSummary[] }) {
	const weaponGroups = useMemo(
		() =>
			groupBy(
				weaponList,
				({ weaponCategory, handsRequired, size }) =>
					`${weaponCategory} ${handsRequired}${size === 'Medium' ? '' : ` (${size})`}`
			),
		[weaponList]
	);
	const weaponSort = sortBy<LegacyWeaponSummary>([({ name }) => name]);
	return (
		<div className="full-page mt-4">
			<MainHeader>Weapon List</MainHeader>
			{sortBy<string>(
				[
					(groupName) => weaponGroups[groupName][0].weaponCategory,
					(groupName) => weaponGroups[groupName][0].handsRequired,
					(groupName) => (weaponGroups[groupName][0].size === 'Medium' ? '' : weaponGroups[groupName][0].size),
				],
				Object.keys(weaponGroups)
			).map((groupName) => (
				<Fragment key={groupName}>
					<h2 className="font-header font-bold mt-4 first:mt-0 text-lg">{groupName}</h2>
					<table className="w-full border-collapse">
						<thead>
							<tr className="bg-theme text-white">
								<th className="px-2 font-bold align-bottom">Name</th>
								<th className="px-2 font-bold align-bottom">Prof.</th>
								<th className="px-2 font-bold align-bottom">Damage</th>
								<th className="px-2 font-bold align-bottom">Range</th>
								<th className="px-2 font-bold align-bottom">Price</th>
								<th className="px-2 font-bold align-bottom">Weight</th>
								<th className="px-2 font-bold align-bottom">Group</th>
								<th className="px-2 font-bold align-bottom">Properties</th>
							</tr>
						</thead>
						<tbody>
							{weaponSort(weaponGroups[groupName]).map(
								({ wizardsId, name, proficiencyBonus, damage, range, gold, weight, group, properties }) => (
									<Fragment key={wizardsId}>
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
				</Fragment>
			))}
		</div>
	);
}
