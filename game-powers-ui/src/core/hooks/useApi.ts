import { createContext, useContext } from 'react';
import { DefaultApi } from 'api';

const ApiContext = createContext<DefaultApi>(new DefaultApi({ basePath: '' }));

export const ApiProvider = ApiContext.Provider;

export function useApi() {
	return useContext(ApiContext);
}
