export interface FeatureFlag {
    id: string;
    projectId: string;
    key: string;
    description: string;
    isEnabled: boolean;
    rolloutPercentage: number;
    environment: string;
    targetingRules: TargetingRule[];
}

export interface TargetingRule {
    id: string;
    type: string;
    operator: string;
    value: string;
}

export interface CreateFeatureFlagRequest {
    projectId: string;
    key: string;
    description: string;
    environment: number; // Enum numeric value or string based on backend mapping
}

export interface UpdateFeatureFlagRequest {
    id: string;
    projectId: string;
    description: string;
    isEnabled: boolean;
    rolloutPercentage: number;
    targetingRules?: any[];
}
