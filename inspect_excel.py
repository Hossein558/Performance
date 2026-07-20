import pandas as pd

df = pd.read_excel('Docs/system_software.xlsx')
print("Columns:", df.columns.tolist())
print(df.head())
