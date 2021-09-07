import { ComponentType, ExoticComponent } from 'react';
import { Controller, ControllerRenderProps, FieldValues, Path, UseControllerProps } from 'react-hook-form';

export type ControlledComponentProps<
	P,
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
> = Omit<P, keyof ControllerRenderProps> & UseControllerProps<TFieldValues, TName>;

export type ControlledComponent<P> = <
	TFieldValues extends FieldValues = FieldValues,
	TName extends Path<TFieldValues> = Path<TFieldValues>
>(
	props: ControlledComponentProps<P, TFieldValues, TName>
) => JSX.Element;

export function Controlled<P>(Component: ExoticComponent<P> | ComponentType<P>): ControlledComponent<P> {
	return ({ name, rules, shouldUnregister, defaultValue, control, ...props }) => (
		<Controller
			name={name}
			rules={rules}
			shouldUnregister={shouldUnregister}
			defaultValue={defaultValue}
			control={control}
			render={({ field }) => <Component {...props} {...(field as any)} />}
		/>
	);
}
