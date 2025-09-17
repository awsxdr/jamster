import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import pluginReact from "eslint-plugin-react";
import stylistic from "@stylistic/eslint-plugin";
import { defineConfig, globalIgnores } from "eslint/config";

export default defineConfig([
    globalIgnores(["dist/", "node_modules/"]),
    { 
        files: ["**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"], 
        plugins: { js }, 
        extends: ["js/recommended"], 
        languageOptions: { globals: globals.browser } 
    },
    tseslint.configs.strict,
    tseslint.configs.stylistic,
    pluginReact.configs.flat.recommended,
    pluginReact.configs.flat["jsx-runtime"],
    {
        plugins: {
            '@stylistic': stylistic,
        },
        rules: {
            "@typescript-eslint/consistent-type-definitions": ["error", "type"],
            "@typescript-eslint/no-empty-function": "off",
            "@typescript-eslint/no-unused-vars": ["error", { "argsIgnorePattern": "^_" }],
            "@stylistic/indent": ["error", 4],
        },
    },
]);
